using Fioo.Data;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/candidaturas")]
    [Authorize]
    public class CandidaturasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CandidaturasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidatura>>> GetAll()
        {
            return await _context.Candidaturas
                .Include(c => c.Usuario)
                .Include(c => c.Servico)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Candidatura>> GetById(int id)
        {
            var candidatura = await _context.Candidaturas
                .Include(c => c.Usuario)
                .Include(c => c.Servico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null)
                return NotFound();

            return candidatura;
        }

        [HttpGet("servico/{servicoId}")]
        public async Task<ActionResult<IEnumerable<Candidatura>>> GetByServico(int servicoId)
        {
            return await _context.Candidaturas
                .Where(c => c.ServicoId == servicoId)
                .Include(c => c.Usuario)
                .ToListAsync();
        }

        [HttpGet("em-andamento/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<CandidaturaDto>>> GetEmAndamento(int usuarioId)
        {
            var candidaturas = await _context.Candidaturas
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Servico)
                    .ThenInclude(s => s!.Usuario)
                .Include(c => c.Servico)
                    .ThenInclude(s => s!.Maquinarios)!
                        .ThenInclude(sm => sm.Maquinario)
                .OrderByDescending(c => c.DataCandidatura)
                .ToListAsync();

            var resultado = candidaturas.Select(c => new CandidaturaDto
            {
                Id = c.Id,
                Status = c.Status,
                DataCandidatura = c.DataCandidatura,
                DataAtualizacao = c.DataAtualizacao,
                Servico = new ServicoResumoDto
                {
                    Id = c.Servico!.Id,
                    Titulo = c.Servico.Titulo,
                    Descricao = c.Servico.Descricao,
                    Cidade = c.Servico.Cidade,
                    Estado = c.Servico.Estado,
                    CategoriaServico = c.Servico.CategoriaServico,
                    Valor = c.Servico.Valor,
                    TipoCobranca = c.Servico.TipoCobranca,
                    TipoPrazo = c.Servico.TipoPrazo,
                    DataPrazo = c.Servico.DataPrazo,
                    Status = c.Servico.Status,
                    DataCriacao = c.Servico.DataCriacao,
                    Usuario = new UsuarioResumoDto
                    {
                        Id = c.Servico.Usuario!.Id,
                        Nome = c.Servico.Usuario.Nome,
                        NomeUsuario = c.Servico.Usuario.NomeUsuario,
                        FotoPerfilUrl = c.Servico.Usuario.FotoPerfilUrl,
                        Cidade = c.Servico.Usuario.Cidade,
                        Estado = c.Servico.Usuario.Estado
                    },
                    Maquinarios = c.Servico.Maquinarios?
                        .Where(sm => sm.Maquinario != null)
                        .Select(sm => new MaquinarioResumoDto
                        {
                            Id = sm.Maquinario!.Id,
                            Nome = sm.Maquinario.Nome
                        }).ToList() ?? []
                }
            });

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CriarCandidaturaDto dto)
        {
            // O candidato é sempre o usuário autenticado (claims do token)
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var servico = await _context.Servicos.FindAsync(dto.ServicoId);

            if (servico == null)
                return NotFound("Serviço não encontrado.");

            if (servico.Status != ServicoStatus.Ativo)
                return BadRequest("Não é possível se candidatar a um serviço que não está ativo.");

            if (servico.UsuarioId == userId.Value)
                return BadRequest("Você não pode se candidatar ao seu próprio serviço.");

            var temCostureiroVinculado = await _context.Candidaturas
                .AnyAsync(c => c.ServicoId == dto.ServicoId && c.Status == CandidaturaStatus.Aceita);

            if (temCostureiroVinculado)
                return Conflict("Este serviço já tem um costureiro.");

            var jaExiste = await _context.Candidaturas
                .AnyAsync(c => c.ServicoId == dto.ServicoId && c.UsuarioId == userId.Value);

            if (jaExiste)
                return BadRequest("Usuário já se candidatou para este serviço.");

            var candidatura = new Candidatura
            {
                ServicoId = dto.ServicoId,
                UsuarioId = userId.Value,
                Status = CandidaturaStatus.Pendente,
                DataCandidatura = DateTime.UtcNow
            };

            _context.Candidaturas.Add(candidatura);
            await _context.SaveChangesAsync();

            return Ok(candidatura);
        }

        /// <summary>
        /// Altera o status de uma candidatura pendente.
        /// Recusada: apenas o fornecedor dono do serviço. Cancelada: apenas o próprio candidato.
        /// Para aceitar, use POST /api/servicos/{id}/candidaturas/{candidaturaId}/aceitar.
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CandidaturaStatus status)
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var candidatura = await _context.Candidaturas
                .Include(c => c.Servico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null)
                return NotFound("Candidatura não encontrada.");

            var ehDonoDoServico = candidatura.Servico!.UsuarioId == userId.Value;
            var ehCandidato = candidatura.UsuarioId == userId.Value;

            if (!ehDonoDoServico && !ehCandidato)
                return StatusCode(StatusCodes.Status403Forbidden, "Você não participa desta candidatura.");

            if (candidatura.Status != CandidaturaStatus.Pendente)
                return Conflict("Só é possível alterar candidaturas pendentes.");

            switch (status)
            {
                case CandidaturaStatus.Recusada when !ehDonoDoServico:
                    return StatusCode(StatusCodes.Status403Forbidden, "Apenas o fornecedor dono do serviço pode recusar candidaturas.");
                case CandidaturaStatus.Cancelada when !ehCandidato:
                    return StatusCode(StatusCodes.Status403Forbidden, "Apenas o próprio costureiro pode cancelar a candidatura.");
                case CandidaturaStatus.Recusada:
                case CandidaturaStatus.Cancelada:
                    break;
                case CandidaturaStatus.Aceita:
                    return Conflict("Para aceitar um candidato, use a opção \"Aceitar\" na lista de candidatos do serviço.");
                default:
                    return BadRequest("Status de candidatura inválido.");
            }

            candidatura.Status = status;
            candidatura.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var candidatura = await _context.Candidaturas
                .Include(c => c.Servico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null)
                return NotFound();

            if (candidatura.UsuarioId != userId.Value && candidatura.Servico!.UsuarioId != userId.Value)
                return StatusCode(StatusCodes.Status403Forbidden, "Você não participa desta candidatura.");

            // A candidatura aceita é o vínculo do costureiro com o serviço; não pode ser apagada
            if (candidatura.Status == CandidaturaStatus.Aceita)
                return Conflict("Não é possível excluir uma candidatura aceita.");

            _context.Candidaturas.Remove(candidatura);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetUsuarioIdFromClaims()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
