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
            SenhaHash = GerarHash(dto.Senha),
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

        var hash = GerarHash(dto.Senha);
        if (usuario.SenhaHash != hash)
            return Unauthorized("Credenciais inválidas.");

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

    [HttpGet("costureiros")]
    public async Task<IActionResult> ListarCostureiros()
    {
        var usuarios = await _dbContext.Usuarios.Where(u => u.Tipo == UsuarioTipo.Costureiro).ToListAsync();
        List<ResumoListarUsuarioDto> resumo = ConverterUsuarioParaResumo(usuarios);
        return Ok(resumo);
    }

    [HttpGet("fornecedores")]
    public async Task<IActionResult> ListarFornecedores()
    {
        var usuarios = await _dbContext.Usuarios.Where(u => u.Tipo == UsuarioTipo.Fornecedor).ToListAsync();
        List<ResumoListarUsuarioDto> resumo = ConverterUsuarioParaResumo(usuarios);
        return Ok(resumo);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var usuario = await _dbContext.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var usuario = await _dbContext.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound();

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

    // Método de hash já existente
    private static string GerarHash(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
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

    private static List<ResumoListarUsuarioDto> ConverterUsuarioParaResumo(List<Usuario> usuarios)
    {
        return usuarios.Select(
            u => new ResumoListarUsuarioDto
            {
                Foto = u.FotoPerfilUrl,
                Nome = u.Nome,
                Localizacao = u.Cidade + " - " + u.Estado,
                MediaEstrela = u.MediaAvaliacoes.ToString()
            }).ToList();
    }
}