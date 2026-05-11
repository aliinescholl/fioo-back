using Fioo.Data;
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

        [HttpPost]
        public async Task<ActionResult> Create(Candidatura candidatura)
        {
            var jaExiste = await _context.Candidaturas
                .AnyAsync(c => c.ServicoId == candidatura.ServicoId
                            && c.UsuarioId == candidatura.UsuarioId);

            if (jaExiste)
                return BadRequest("Usuário já se candidatou para este serviço.");

            candidatura.DataCandidatura = DateTime.UtcNow;

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