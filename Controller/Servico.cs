using Fioo.Data;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
                .Where(s => s.Status == ServicoStatus.Ativo && s.UsuarioId != usuarioId)
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

            if (!EstaEmAndamento(servico))
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
                return Forbid("Apenas usuários do tipo Fornecedor podem criar serviços.");

            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            // validações básicas do payload
            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { field = "titulo", message = "Título é obrigatório." });

            if (dto.Valor.HasValue && dto.Valor < 0)
                return BadRequest(new { field = "valor", message = "Valor não pode ser negativo." });

            // Mapear DTO para entidade, populando UsuarioId a partir do token
            // Validar e parsear DataPrazo de forma segura
            if (string.IsNullOrWhiteSpace(dto.DataPrazo))
                return BadRequest(new { field = "dataPrazo", message = "DataPrazo é obrigatória." });

            if (!DateOnly.TryParse(dto.DataPrazo, out var dataPrazo))
                return BadRequest(new { field = "dataPrazo", message = "Formato de data inválido." });

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
                Status = dto.Status,
                DataCriacao = DateTime.UtcNow
            };

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = servico.Id }, servico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Servico servico)
        {
            if (id != servico.Id)
                return BadRequest();

            var existing = await _context.Servicos.FindAsync(id);

            if (existing == null)
                return NotFound();

            // Apenas o proprietário (fornecedor dono) pode atualizar
            var userId = GetUsuarioIdFromClaims();
            if (userId == null || existing.UsuarioId != userId.Value)
                return Forbid("Apenas o fornecedor proprietário pode editar este serviço.");

            existing.Titulo = servico.Titulo;
            existing.Descricao = servico.Descricao;
            existing.Cidade = servico.Cidade;
            existing.Estado = servico.Estado;
            existing.TipoCobranca = servico.TipoCobranca;
            existing.CategoriaServico = servico.CategoriaServico;
            existing.Valor = servico.Valor;
            existing.TipoPrazo = servico.TipoPrazo;
            existing.DataPrazo = servico.DataPrazo;
            existing.Status = servico.Status;

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
                return Forbid("Apenas o fornecedor proprietário pode deletar este serviço.");

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

        private static UsuarioResumoDto ToUsuarioResumoDto(Usuario u) => new()
        {
            Id = u.Id,
            Nome = u.Nome,
            NomeUsuario = u.NomeUsuario,
            FotoPerfilUrl = u.FotoPerfilUrl,
            Cidade = u.Cidade,
            Estado = u.Estado
        };

        // Até a padronização de status (Fase 2), "Ativo" e "EmAndamento" equivalem a "Em andamento"
        private static bool EstaEmAndamento(Servico s) =>
            s.Status == ServicoStatus.Ativo || s.Status == ServicoStatus.EmAndamento;

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