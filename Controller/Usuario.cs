using Fioo.Controller.DTOs;
using Fioo.Data;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Services;
using Fioo.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Fioo.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _appConfiguration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly CnpjWsService _cnpjWsService;

    // Upload rules
    private readonly string[] _usuarioPermittedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB

    public UsuariosController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment env, CnpjWsService cnpjWsService)
    {
        _dbContext = context;
        _appConfiguration = configuration;
        _webHostEnvironment = env;
        _cnpjWsService = cnpjWsService;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CriarUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest("Email e senha são obrigatórios.");

        // Validação de email
        if (!ValidationHelpers.ValidarEmail(dto.Email))
            return BadRequest("Email inválido.");

        // Validação de força de senha
        if (!ValidationHelpers.ValidarSenhaForte(dto.Senha))
            return BadRequest("Senha fraca. Deve ter pelo menos 8 caracteres, conter letras maiúsculas, minúsculas e um caractere especial.");

        if (await _dbContext.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email já cadastrado");

        var usuario = new Usuario
        {
            Email = dto.Email,
            SenhaHash = SenhaHasher.Gerar(dto.Senha),
            Ativo = true
        };

        // ── Fluxo de Fornecedor (EhCostureiro = false) ────────────────────────
        if (!dto.EhCostureiro)
        {
            if (string.IsNullOrWhiteSpace(dto.CNPJ))
                return BadRequest("CNPJ é obrigatório para fornecedores.");

            // Remove máscara: pontos, barras e hífens
            var cnpjLimpo = Regex.Replace(dto.CNPJ, @"[^\d]", "");

            if (cnpjLimpo.Length != 14)
                return BadRequest("CNPJ inválido. Informe os 14 dígitos.");

            CnpjWsResponse? dadosCnpj;
            try
            {
                dadosCnpj = await _cnpjWsService.ConsultarAsync(cnpjLimpo);
            }
            catch (CnpjWsException ex) when (ex.Erro == CnpjWsErro.NaoEncontrado)
            {
                return BadRequest("CNPJ não encontrado na base de dados.");
            }
            catch (CnpjWsException ex) when (ex.Erro == CnpjWsErro.RateLimitAtingido)
            {
                return StatusCode(429, "Limite de consultas ao serviço de CNPJ atingido. Tente novamente em 1 minuto.");
            }
            catch (CnpjWsException ex)
            {
                return BadRequest($"Erro ao consultar CNPJ: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(503, "Serviço de consulta de CNPJ indisponível no momento.");
            }

            if (dadosCnpj == null)
                return BadRequest("Não foi possível obter dados do CNPJ.");

            // Valida situação cadastral
            var situacao = dadosCnpj.Estabelecimento?.SituacaoCadastral ?? "";
            if (!situacao.Equals("Ativa", StringComparison.OrdinalIgnoreCase))
                return BadRequest($"CNPJ com situação cadastral '{situacao}'. Apenas CNPJs com situação 'Ativa' são aceitos.");

            // Preenche dados do fornecedor a partir da API
            usuario.Tipo        = UsuarioTipo.Fornecedor;
            usuario.CpfCnpj     = cnpjLimpo;
            usuario.RazaoSocial = dadosCnpj.RazaoSocial;
            usuario.NomeFantasia = dadosCnpj.Estabelecimento?.NomeFantasia;
            usuario.EmailContato = dadosCnpj.Estabelecimento?.Email;
            usuario.Cidade      = dadosCnpj.Estabelecimento?.Cidade?.Nome;
            usuario.Estado      = dadosCnpj.Estabelecimento?.Estado?.Sigla;

            // Monta telefone com DDD se disponível
            var ddd = dadosCnpj.Estabelecimento?.Ddd1;
            var tel = dadosCnpj.Estabelecimento?.Telefone1;
            if (!string.IsNullOrWhiteSpace(tel))
                usuario.Telefone = string.IsNullOrWhiteSpace(ddd) ? tel : $"({ddd}) {tel}";

            // Nome: usa razão social como nome principal
            var nomeBase = dadosCnpj.RazaoSocial ?? dto.CNPJ;
            usuario.Nome = nomeBase;
        }
        // ── Fluxo de Costureiro (EhCostureiro = true) ─────────────────────────
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest("Nome é obrigatório para costureiros.");

            usuario.Tipo = UsuarioTipo.Costureiro;
            usuario.Nome = dto.Nome;
        }

        // Gera NomeUsuario único a partir do nome
        var baseUsername = Regex.Replace((usuario.Nome ?? dto.Email).ToLower(), @"[^a-z0-9]", "");
        if (string.IsNullOrWhiteSpace(baseUsername))
            baseUsername = dto.Email.Split('@')[0];

        var username = baseUsername;
        var suffix = 1;
        while (await _dbContext.Usuarios.AnyAsync(u => u.NomeUsuario != null && u.NomeUsuario == username))
        {
            username = $"{baseUsername}{suffix}";
            suffix++;
        }
        usuario.NomeUsuario = username;

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest("Email e senha são obrigatórios.");

        // Validar formato de email antes de consultar DB
        if (!ValidationHelpers.ValidarEmail(dto.Email))
            return BadRequest("Email inválido.");

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (usuario == null)
            return Unauthorized("Credenciais inválidas.");

        if (!SenhaHasher.Verificar(dto.Senha, usuario.SenhaHash))
            return Unauthorized("Credenciais inválidas.");

        // Contas com hash antigo (SHA-256 sem salt) passam para PBKDF2 ao entrar
        if (SenhaHasher.PrecisaAtualizar(usuario.SenhaHash))
        {
            usuario.SenhaHash = SenhaHasher.Gerar(dto.Senha);
            await _dbContext.SaveChangesAsync();
        }

        var token = GerarJwt(usuario);
        return Ok(new { token });
    }

    [HttpPost("verificar-email")]
    public async Task<IActionResult> VerificarEmail([FromBody] EmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email é obrigatório.");

        if (!ValidationHelpers.ValidarEmail(dto.Email))
            return BadRequest("Email inválido.");

        var exists = await _dbContext.Usuarios.AnyAsync(u => u.Email == dto.Email);
        return Ok(exists);
    }

    private static readonly string[] OrdenacoesUsuarios = ["relevantes", "avaliacao-alta", "avaliacao-baixa", "az", "za"];

    [HttpGet("costureiros")]
    [Authorize]
    public Task<ActionResult<PaginaUsuariosDto>> ListarCostureiros([FromQuery] UsuarioFiltroDto filtro) =>
        ListarPorTipo(UsuarioTipo.Costureiro, filtro);

    [HttpGet("fornecedores")]
    [Authorize]
    public Task<ActionResult<PaginaUsuariosDto>> ListarFornecedores([FromQuery] UsuarioFiltroDto filtro) =>
        ListarPorTipo(UsuarioTipo.Fornecedor, filtro);

    /// <summary>
    /// Tela Encontrar: usuários de um tipo com média/total das avaliações recebidas nesse papel,
    /// filtros, ordenação e paginação numa única consulta. Busca e ordem alfabética ignoram
    /// maiúsculas e acentos (normalizar_texto). Quem não tem avaliação fica sempre depois dos
    /// avaliados nas ordenações por nota e só é excluído quando há filtro de avaliação mínima.
    /// </summary>
    private async Task<ActionResult<PaginaUsuariosDto>> ListarPorTipo(UsuarioTipo tipo, UsuarioFiltroDto filtro)
    {
        if (filtro.AvaliacaoMin is < 1 or > 5)
            return BadRequest(new { field = "avaliacaoMin", message = "A avaliação mínima deve ser de 1 a 5 estrelas." });
        if (filtro.Uf != null && filtro.Uf.Trim().Length is not (0 or 2))
            return BadRequest(new { field = "uf", message = "UF inválida." });
        if (filtro.Ordenacao != null && !OrdenacoesUsuarios.Contains(filtro.Ordenacao))
            return BadRequest(new { field = "ordenacao", message = "Ordenação inválida." });
        if (filtro.Pagina < 1)
            return BadRequest(new { field = "pagina", message = "Página inválida." });
        if (filtro.TamanhoPagina is < 1 or > 50)
            return BadRequest(new { field = "tamanhoPagina", message = "O tamanho da página deve ser entre 1 e 50." });

        var usuarios = _dbContext.Usuarios.AsNoTracking().Where(u => u.Tipo == tipo);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            usuarios = usuarios.Where(u =>
                AppDbContext.NormalizarTexto(u.Nome).Contains(AppDbContext.NormalizarTexto(filtro.Busca))
                || (u.NomeFantasia != null && AppDbContext.NormalizarTexto(u.NomeFantasia).Contains(AppDbContext.NormalizarTexto(filtro.Busca)))
                || AppDbContext.NormalizarTexto(u.NomeUsuario).Contains(AppDbContext.NormalizarTexto(filtro.Busca)));

        if (!string.IsNullOrWhiteSpace(filtro.Uf))
        {
            var uf = filtro.Uf.Trim().ToUpperInvariant();
            usuarios = usuarios.Where(u => u.Estado == uf);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
            usuarios = usuarios.Where(u => u.Cidade != null
                && AppDbContext.NormalizarTexto(u.Cidade).Contains(AppDbContext.NormalizarTexto(filtro.Cidade)));

        // Média e total só das avaliações recebidas no papel listado
        var comNotas = usuarios.Select(u => new
        {
            Usuario = u,
            NomeOrdenacao = AppDbContext.NormalizarTexto(u.Nome),
            Media = u.AvaliacoesRecebidas!.Where(a => a.PapelAvaliado == tipo).Average(a => (double?)a.Nota),
            Total = u.AvaliacoesRecebidas!.Count(a => a.PapelAvaliado == tipo)
        });

        if (filtro.AvaliacaoMin.HasValue)
            comNotas = comNotas.Where(x => x.Media >= filtro.AvaliacaoMin.Value);

        // Desempate final por Id para a paginação ser estável
        comNotas = (filtro.Ordenacao ?? "relevantes") switch
        {
            "avaliacao-baixa" => comNotas.OrderBy(x => x.Media == null).ThenBy(x => x.Media).ThenByDescending(x => x.Total).ThenBy(x => x.Usuario.Id),
            "az" => comNotas.OrderBy(x => x.NomeOrdenacao).ThenBy(x => x.Usuario.Id),
            "za" => comNotas.OrderByDescending(x => x.NomeOrdenacao).ThenByDescending(x => x.Usuario.Id),
            // "Mais relevantes" e "Avaliação mais alta": maior média, depois mais avaliações; sem avaliação por último
            _ => comNotas.OrderBy(x => x.Media == null).ThenByDescending(x => x.Media).ThenByDescending(x => x.Total)
                .ThenBy(x => x.NomeOrdenacao).ThenBy(x => x.Usuario.Id)
        };

        var itens = await comNotas
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina + 1)
            .Select(x => new ResumoListarUsuarioDto
            {
                Id = x.Usuario.Id,
                Nome = x.Usuario.Nome,
                NomeUsuario = x.Usuario.NomeUsuario,
                Foto = x.Usuario.FotoPerfilUrl,
                Localizacao = x.Usuario.Cidade != null && x.Usuario.Estado != null
                    ? x.Usuario.Cidade + " - " + x.Usuario.Estado
                    : x.Usuario.Cidade ?? x.Usuario.Estado,
                Media = x.Media,
                TotalAvaliacoes = x.Total
            })
            .ToListAsync();

        foreach (var item in itens.Where(i => i.Media.HasValue))
            item.Media = Math.Round(item.Media!.Value, 1);

        return Ok(new PaginaUsuariosDto
        {
            Itens = itens.Take(filtro.TamanhoPagina).ToList(),
            Pagina = filtro.Pagina,
            TemMais = itens.Count > filtro.TamanhoPagina
        });
    }

    /// <summary>
    /// Dados completos só para o próprio usuário; para os demais, apenas os dados públicos.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id)
    {
        if (GetUsuarioIdFromClaims() != id)
            return (await ObterPerfilPublico(id)).Result!;

        var usuario = await _dbContext.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(usuario);
    }

    [HttpGet("{id}/publico")]
    [Authorize]
    public async Task<ActionResult<PerfilPublicoDto>> ObterPerfilPublico(int id)
    {
        var perfil = await _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new PerfilPublicoDto
            {
                Id = u.Id,
                Nome = u.Nome,
                NomeUsuario = u.NomeUsuario,
                FotoPerfilUrl = u.FotoPerfilUrl,
                Cidade = u.Cidade,
                Estado = u.Estado,
                Tipo = u.Tipo,
                AnosExperiencia = u.AnosExperiencia
            })
            .FirstOrDefaultAsync();

        if (perfil == null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(perfil);
    }

    /// <summary>
    /// Exclui a própria conta. Contas com histórico (candidaturas, avaliações, denúncias ou
    /// serviço com costureiro vinculado) não podem ser excluídas, para não apagar dados de outras pessoas.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Deletar(int id)
    {
        if (GetUsuarioIdFromClaims() != id)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Você só pode excluir a sua própria conta." });

        var usuario = await _dbContext.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { message = "Usuário não encontrado." });

        var temHistorico =
            await _dbContext.Candidaturas.AnyAsync(c => c.UsuarioId == id
                || (c.Servico!.UsuarioId == id && c.Status == CandidaturaStatus.Aceita))
            || await _dbContext.Avaliacoes.AnyAsync(a => a.AvaliadorId == id || a.AvaliadoId == id)
            || await _dbContext.Denuncias.AnyAsync(d => d.DenuncianteId == id || d.DenunciadoId == id);

        if (temHistorico)
            return Conflict(new { message = "Não é possível excluir esta conta porque ela tem histórico de candidaturas, avaliações ou denúncias." });

        _dbContext.Usuarios.Remove(usuario);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUsuarioIdFromClaims();
        if (userId == null) return Unauthorized();

        var usuario = await _dbContext.Usuarios
            .Include(u => u.Portfolios)
            .Include(u => u.Maquinarios)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (usuario == null) return NotFound();

        return Ok(usuario);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> AtualizarPerfil([FromForm] AtualizarPerfilDto dto)
    {
        var userId = GetUsuarioIdFromClaims();
        if (userId == null) return Unauthorized();

        var usuario = await _dbContext.Usuarios
            .Include(u => u.Portfolios)
            .Include(u => u.Maquinarios)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (usuario == null) return NotFound();

        // Validações essenciais
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
        {
            if (!ValidationHelpers.ValidarEmail(dto.Email))
                return BadRequest("Email inválido.");

            if (await _dbContext.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != usuario.Id))
                return BadRequest("Email já utilizado por outro usuário.");
            usuario.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.NomeUsuario) && dto.NomeUsuario != usuario.NomeUsuario)
        {
            if (await _dbContext.Usuarios.AnyAsync(u => u.NomeUsuario == dto.NomeUsuario && u.Id != usuario.Id))
                return BadRequest("Nome de usuário já em uso.");
            usuario.NomeUsuario = dto.NomeUsuario;
        }

        if (!string.IsNullOrWhiteSpace(dto.Nome))
            usuario.Nome = dto.Nome;

        if (!string.IsNullOrWhiteSpace(dto.NomeSocial))
            usuario.FotoPerfilUrl = usuario.FotoPerfilUrl; // semântica: apenas armazenar NomeSocial em DB se existir campo (ajuste se necessário)

        if (!string.IsNullOrWhiteSpace(dto.NomeSocial))
        {
            // Se a entidade Usuario tiver um campo NomeSocial, atribua aqui.
            // Exemplo: usuario.NomeSocial = dto.NomeSocial;
        }

        if (!string.IsNullOrWhiteSpace(dto.Pronome))
        {
            // Se a entidade Usuario tiver um campo Pronome, atribua aqui.
            // Exemplo: usuario.Pronome = dto.Pronome;
        }

        if (!string.IsNullOrWhiteSpace(dto.CpfCnpj))
        {
            if (!ValidationHelpers.ValidarCpfOuCnpj(dto.CpfCnpj))
                return BadRequest("CPF/CNPJ inválido.");
            usuario.CpfCnpj = dto.CpfCnpj;
        }

        if (!string.IsNullOrWhiteSpace(dto.Telefone))
            usuario.Telefone = dto.Telefone;

        if (dto.TelefoneVisivel.HasValue)
            usuario.TelefoneVisivel = dto.TelefoneVisivel.Value;

        if (dto.ServicoPrestado.HasValue)
            usuario.Tipo = dto.ServicoPrestado.Value;

        if (dto.AnosExperiencia.HasValue)
            usuario.AnosExperiencia = dto.AnosExperiencia;

        // Endereço: se desejar mapear para campos Cidade/Estado já existentes:
        if (dto.Endereco != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Endereco.Cidade))
                usuario.Cidade = dto.Endereco.Cidade;
            if (!string.IsNullOrWhiteSpace(dto.Endereco.Estado))
                usuario.Estado = dto.Endereco.Estado;
            // demais campos podem ser salvos em um novo objeto Endereco na entidade se existir
        }

        // Maquinários: sincronizar associações (limpa e adiciona)
        if (dto.MaquinarioIds != null)
        {
            // Remove associações existentes
            var existentes = _dbContext.UsuarioMaquinarios.Where(um => um.UsuarioId == usuario.Id);
            _dbContext.UsuarioMaquinarios.RemoveRange(existentes);

            // Adiciona novas referências (valida existência de Maquinario)
            var maquinas = await _dbContext.Maquinarios
                .Where(m => dto.MaquinarioIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            foreach (var mId in maquinas)
            {
                _dbContext.UsuarioMaquinarios.Add(new UsuarioMaquinario
                {
                    UsuarioId = usuario.Id,
                    MaquinarioId = mId
                });
            }
        }

        // Foto de perfil: upload seguro
        if (dto.FotoPerfil != null)
        {
            var foto = dto.FotoPerfil;
            if (foto.Length > MaxFileSize)
                return BadRequest("Arquivo da foto excede o tamanho máximo permitido (2MB).");

            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_usuarioPermittedExtensions.Contains(ext))
                return BadRequest("Formato de foto inválido. Use .jpg, .jpeg ou .png.");

            // Gera nome seguro
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await foto.CopyToAsync(stream);
            }

            // Define URL pública relativa
            usuario.FotoPerfilUrl = $"/uploads/avatars/{fileName}";
        }

        // Portfólio: salvar arquivos (placeholder: cria registros em Portfolios)
        if (dto.Portfolios != null && dto.Portfolios.Count > 0)
        {
            var portDir = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "portfolios", usuario.Id.ToString());
            Directory.CreateDirectory(portDir);

            foreach (var f in dto.Portfolios)
            {
                if (f.Length == 0) continue;
                if (f.Length > MaxFileSize) continue;

                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !_usuarioPermittedExtensions.Contains(ext)) continue;

                var fname = $"{Guid.NewGuid()}{ext}";
                var dest = Path.Combine(portDir, fname);
                using (var stream = System.IO.File.Create(dest))
                {
                    await f.CopyToAsync(stream);
                }

                var portfolio = new Portfolio
                {
                    UsuarioId = usuario.Id,
                    FotoUrl = $"/uploads/portfolios/{usuario.Id}/{fname}",
                    DataUpload = DateTime.UtcNow
                };

                _dbContext.Portfolios.Add(portfolio);
            }
        }

        // Salva alterações
        await _dbContext.SaveChangesAsync();

        // Retorna payload simples para front exibir feedback: mensagem + icone
        return Ok(new { message = "Alterações salvas", icon = "check" });
    }

    // helpers para extrair informações do token
    private int? GetUsuarioIdFromClaims()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(sub, out var id))
            return id;
        return null;
    }

    // GerarJwt existente (preserve se já tiver)
    private string GerarJwt(Usuario usuario)
    {
        var key = _appConfiguration["Jwt:Key"];
        var issuer = _appConfiguration["Jwt:Issuer"] ?? "fioo";
        var audience = _appConfiguration["Jwt:Audience"] ?? "fioo";
        var expiresMinutes = int.TryParse(_appConfiguration["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? ""));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nome", usuario.Nome ?? string.Empty),
            new Claim("nome_usuario", usuario.NomeUsuario ?? string.Empty),
            new Claim("tipo", usuario.Tipo.ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}