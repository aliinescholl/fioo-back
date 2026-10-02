using Fioo.Data;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Fioo.Utils;
using System.Globalization;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/servicos")]
    [Authorize]
    public class ServicosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoResumoDto>>> GetAll([FromQuery] int usuarioId)
        {
            var servicos = await _context.Servicos
                .Where(s => s.Status == ServicoStatus.EmAndamento && s.UsuarioId != usuarioId)
                .Include(s => s.Usuario)
                .Include(s => s.Maquinarios)!
                    .ThenInclude(sm => sm.Maquinario)
                .Include(s => s.Candidaturas!.Where(c => c.Status == CandidaturaStatus.Aceita))
                    .ThenInclude(c => c.Usuario)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();

            return Ok(servicos.Select(ToResumoDto));
        }

        [HttpGet("meus/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<ServicoResumoDto>>> GetMeus(int usuarioId)
        {
            var servicos = await _context.Servicos
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.Usuario)
                .Include(s => s.Maquinarios)!
                    .ThenInclude(sm => sm.Maquinario)
                .Include(s => s.Candidaturas!.Where(c => c.Status == CandidaturaStatus.Aceita))
                    .ThenInclude(c => c.Usuario)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();

            return Ok(servicos.Select(ToResumoDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServicoResumoDto>> GetById(int id)
        {
            var servico = await _context.Servicos
                .Include(s => s.Usuario)
                .Include(s => s.Maquinarios)!
                    .ThenInclude(sm => sm.Maquinario)
                .Include(s => s.Candidaturas!.Where(c => c.Status == CandidaturaStatus.Aceita))
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null)
                return NotFound();

            return Ok(ToResumoDto(servico));
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Servico>>> GetByUsuario(int usuarioId)
        {
            return await _context.Servicos
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.Usuario)
                .ToListAsync();
        }

        /// <summary>
        /// Lista os candidatos de um serviço. Apenas o fornecedor dono do serviço pode ver.
        /// </summary>
        [HttpGet("{id}/candidaturas")]
        public async Task<ActionResult<IEnumerable<CandidatoDto>>> GetCandidatos(int id)
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var donoId = await _context.Servicos
                .Where(s => s.Id == id)
                .Select(s => (int?)s.UsuarioId)
                .FirstOrDefaultAsync();

            if (donoId == null)
                return NotFound(new { message = "Serviço não encontrado." });

            if (donoId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas o fornecedor dono do serviço pode ver os candidatos." });

            var candidatos = await _context.Candidaturas
                .AsNoTracking()
                .Where(c => c.ServicoId == id)
                .OrderByDescending(c => c.Status == CandidaturaStatus.Aceita)
                .ThenBy(c => c.DataCandidatura)
                .ThenBy(c => c.Id)
                .Select(c => new CandidatoDto
                {
                    CandidaturaId = c.Id,
                    Status = c.Status,
                    DataCandidatura = c.DataCandidatura,
                    Usuario = new UsuarioResumoDto
                    {
                        Id = c.Usuario!.Id,
                        Nome = c.Usuario.Nome,
                        NomeUsuario = c.Usuario.NomeUsuario,
                        FotoPerfilUrl = c.Usuario.FotoPerfilUrl,
                        Cidade = c.Usuario.Cidade,
                        Estado = c.Usuario.Estado
                    }
                })
                .ToListAsync();

            return Ok(candidatos);
        }

        /// <summary>
        /// Aceita um candidato: a candidatura passa a "Aceita" e vincula o costureiro ao serviço.
        /// As demais candidaturas pendentes do serviço são recusadas automaticamente.
        /// </summary>
        [HttpPost("{id}/candidaturas/{candidaturaId}/aceitar")]
        public async Task<IActionResult> AceitarCandidatura(int id, int candidaturaId)
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var servico = await _context.Servicos
                .Include(s => s.Candidaturas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null)
                return NotFound(new { message = "Serviço não encontrado." });

            if (servico.UsuarioId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas o fornecedor dono do serviço pode aceitar candidatos." });

            var candidatura = servico.Candidaturas!.FirstOrDefault(c => c.Id == candidaturaId);
            if (candidatura == null)
                return NotFound(new { message = "Candidatura não encontrada neste serviço." });

            if (servico.Status != ServicoStatus.EmAndamento)
                return Conflict(new { message = "Só é possível aceitar candidatos em serviços em andamento." });

            if (servico.Candidaturas!.Any(c => c.Status == CandidaturaStatus.Aceita))
                return Conflict(new { message = "Este serviço já tem um costureiro." });

            if (candidatura.Status != CandidaturaStatus.Pendente)
                return Conflict(new { message = "Só é possível aceitar candidaturas pendentes." });

            var agora = DateTime.UtcNow;
            candidatura.Status = CandidaturaStatus.Aceita;
            candidatura.DataAtualizacao = agora;

            foreach (var outra in servico.Candidaturas!.Where(c => c.Id != candidatura.Id && c.Status == CandidaturaStatus.Pendente))
            {
                outra.Status = CandidaturaStatus.Recusada;
                outra.DataAtualizacao = agora;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Outro candidato foi aceito ao mesmo tempo (índice único parcial IX_Candidaturas_ServicoId_Aceita)
                return Conflict(new { message = "Este serviço já tem um costureiro." });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Servico>> Create([FromBody] ServicoDto dto)
        {
            // valida token / tipo do usuário
            var tipo = GetUsuarioTipoFromClaims();
            if (tipo == null || tipo != UsuarioTipo.Fornecedor)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas usuários do tipo Fornecedor podem criar serviços." });

            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var erro = ValidarServico(dto, out var dataPrazo);
            if (erro != null)
                return erro;

            var dataCriacao = DateTime.UtcNow;

            // Mapear DTO para entidade, populando UsuarioId a partir do token
            var servico = new Servico
            {
                UsuarioId = userId.Value,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                TipoCobranca = dto.TipoCobranca,
                CategoriaServico = dto.CategoriaServico,
                Valor = dto.Valor,
                TipoPrazo = dto.TipoPrazo,
                DataPrazo = dataPrazo,
                DataReferenciaPrazo = PrazoHelper.CalcularDataReferencia(dto.TipoPrazo, dataPrazo, dataCriacao),
                Status = ServicoStatus.EmAndamento, // todo serviço publicado nasce "Em andamento"
                DataCriacao = dataCriacao
            };

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = servico.Id }, servico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServicoDto dto)
        {
            var existing = await _context.Servicos.FindAsync(id);

            if (existing == null)
                return NotFound(new { message = "Serviço não encontrado." });

            // Apenas o proprietário (fornecedor dono) pode atualizar
            var userId = GetUsuarioIdFromClaims();
            if (userId == null || existing.UsuarioId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas o fornecedor proprietário pode editar este serviço." });

            var erro = ValidarServico(dto, out var dataPrazo);
            if (erro != null)
                return erro;

            existing.Titulo = dto.Titulo;
            existing.Descricao = dto.Descricao;
            existing.Cidade = dto.Cidade;
            existing.Estado = dto.Estado;
            existing.TipoCobranca = dto.TipoCobranca;
            existing.CategoriaServico = dto.CategoriaServico;
            existing.Valor = dto.Valor;
            existing.TipoPrazo = dto.TipoPrazo;
            existing.DataPrazo = dataPrazo;
            existing.DataReferenciaPrazo = PrazoHelper.CalcularDataReferencia(dto.TipoPrazo, dataPrazo, existing.DataCriacao);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Altera o status do serviço (só o fornecedor dono). Ver ServicoStatusRegras.
        /// Concluir exige costureiro vinculado (candidatura aceita).
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusServicoDto dto)
        {
            if (!Enum.IsDefined(dto.Status))
                return BadRequest(new { field = "status", message = "Status inválido." });

            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { message = "Serviço não encontrado." });

            var userId = GetUsuarioIdFromClaims();
            if (userId == null || servico.UsuarioId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas o fornecedor dono do serviço pode alterar o status." });

            if (!ServicoStatusRegras.TransicaoPermitida(servico.Status, dto.Status))
            {
                var mensagem = servico.Status == ServicoStatus.EmAndamento
                    ? "O serviço já está em andamento."
                    : "Este serviço já foi encerrado e o status não pode mais ser alterado.";
                return Conflict(new { message = mensagem });
            }

            if (dto.Status == ServicoStatus.Concluido)
            {
                var temCostureiro = await _context.Candidaturas
                    .AnyAsync(c => c.ServicoId == id && c.Status == CandidaturaStatus.Aceita);

                if (!temCostureiro)
                    return UnprocessableEntity(new { message = "Para concluir o serviço, aceite primeiro um costureiro." });
            }

            servico.Status = dto.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var servico = await _context.Servicos.FindAsync(id);

            if (servico == null)
                return NotFound();

            var userId = GetUsuarioIdFromClaims();
            if (userId == null || servico.UsuarioId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas o fornecedor proprietário pode deletar este serviço." });

            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static ServicoResumoDto ToResumoDto(Servico s) => new()
        {
            Id = s.Id,
            Titulo = s.Titulo,
            Descricao = s.Descricao,
            Cidade = s.Cidade,
            Estado = s.Estado,
            CategoriaServico = s.CategoriaServico,
            Valor = s.Valor,
            TipoCobranca = s.TipoCobranca,
            TipoPrazo = s.TipoPrazo,
            DataPrazo = s.DataPrazo,
            Status = s.Status,
            DataCriacao = s.DataCriacao,
            Usuario = ToUsuarioResumoDto(s.Usuario!),
            CostureiroVinculado = s.Candidaturas?
                .Where(c => c.Status == CandidaturaStatus.Aceita && c.Usuario != null)
                .Select(c => ToUsuarioResumoDto(c.Usuario!))
                .FirstOrDefault(),
            Maquinarios = s.Maquinarios?
                .Where(sm => sm.Maquinario != null)
                .Select(sm => new MaquinarioResumoDto
                {
                    Id = sm.Maquinario!.Id,
                    Nome = sm.Maquinario.Nome
                }).ToList() ?? []
        };

        /// <summary>
        /// Valida os campos do formulário de serviço (cadastro e edição).
        /// A data do prazo só é obrigatória e só é guardada quando o prazo é "Data Específica".
        /// </summary>
        private static BadRequestObjectResult? ValidarServico(ServicoDto dto, out DateOnly? dataPrazo)
        {
            dataPrazo = null;

            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return new BadRequestObjectResult(new { field = "titulo", message = "Título é obrigatório." });

            if (!Enum.IsDefined(dto.TipoCobranca))
                return new BadRequestObjectResult(new { field = "tipoCobranca", message = "Tipo de cobrança inválido." });

            if (dto.Valor.HasValue && dto.Valor < 0)
                return new BadRequestObjectResult(new { field = "valor", message = "Valor não pode ser negativo." });

            if (dto.TipoPrazo == null)
                return new BadRequestObjectResult(new { field = "tipoPrazo", message = "Escolha o prazo de entrega." });

            if (!Enum.IsDefined(dto.TipoPrazo.Value))
                return new BadRequestObjectResult(new { field = "tipoPrazo", message = "Prazo de entrega inválido." });

            // Semanal, Quinzenal e Mensal não têm data: qualquer data enviada é ignorada
            if (dto.TipoPrazo != PrazoTipo.DataEspecifica)
                return null;

            if (string.IsNullOrWhiteSpace(dto.DataPrazo))
                return new BadRequestObjectResult(new { field = "dataPrazo", message = "Informe a data do prazo." });

            if (!DateOnly.TryParseExact(dto.DataPrazo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                return new BadRequestObjectResult(new { field = "dataPrazo", message = "Data do prazo inválida." });

            dataPrazo = data;
            return null;
        }

        private static UsuarioResumoDto ToUsuarioResumoDto(Usuario u) => new()
        {
            Id = u.Id,
            Nome = u.Nome,
            NomeUsuario = u.NomeUsuario,
            FotoPerfilUrl = u.FotoPerfilUrl,
            Cidade = u.Cidade,
            Estado = u.Estado
        };

        private int? GetUsuarioIdFromClaims()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }

        private UsuarioTipo? GetUsuarioTipoFromClaims()
        {
            var tipoStr = User.FindFirst("tipo")?.Value;
            if (string.IsNullOrEmpty(tipoStr))
                return null;

            if (Enum.TryParse<UsuarioTipo>(tipoStr, out var tipo))
                return tipo;

            return null;
        }
    }
}