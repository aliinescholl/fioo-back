using Fioo.Data;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/candidaturas")]
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
            var servico = await _context.Servicos.FindAsync(dto.ServicoId);

            if (servico == null)
                return NotFound("Serviço não encontrado.");

            if (servico.Status != ServicoStatus.Ativo)
                return BadRequest("Não é possível se candidatar a um serviço que não está ativo.");

            if (servico.UsuarioId == dto.UsuarioId)
                return BadRequest("Você não pode se candidatar ao seu próprio serviço.");

            var jaExiste = await _context.Candidaturas
                .AnyAsync(c => c.ServicoId == dto.ServicoId && c.UsuarioId == dto.UsuarioId);

            if (jaExiste)
                return BadRequest("Usuário já se candidatou para este serviço.");

            var candidatura = new Candidatura
            {
                ServicoId = dto.ServicoId,
                UsuarioId = dto.UsuarioId,
                Status = CandidaturaStatus.Pendente,
                DataCandidatura = DateTime.UtcNow
            };

            _context.Candidaturas.Add(candidatura);
            await _context.SaveChangesAsync();

            return Ok(candidatura);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CandidaturaStatus status)
        {
            var candidatura = await _context.Candidaturas.FindAsync(id);

            if (candidatura == null)
                return NotFound();

            candidatura.Status = status;
            candidatura.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var candidatura = await _context.Candidaturas.FindAsync(id);

            if (candidatura == null)
                return NotFound();

            _context.Candidaturas.Remove(candidatura);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}