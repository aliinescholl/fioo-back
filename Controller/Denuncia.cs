using Fioo.Data;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DenunciasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DenunciasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Denuncia>>> GetAll()
        {
            return await _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.Denunciado)
                .OrderByDescending(d => d.DataDenuncia)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Denuncia>> GetById(int id)
        {
            var denuncia = await _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.Denunciado)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia == null)
                return NotFound();

            return denuncia;
        }

        [HttpGet("denunciante/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Denuncia>>> GetByDenunciante(int usuarioId)
        {
            return await _context.Denuncias
                .Where(d => d.DenuncianteId == usuarioId)
                .ToListAsync();
        }

        [HttpGet("denunciado/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Denuncia>>> GetByDenunciado(int usuarioId)
        {
            return await _context.Denuncias
                .Where(d => d.DenunciadoId == usuarioId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Denuncia denuncia)
        {
            if (denuncia.DenuncianteId == denuncia.DenunciadoId)
                return BadRequest("Você não pode denunciar a si mesmo.");

            var jaExiste = await _context.Denuncias.AnyAsync(d =>
                d.DenuncianteId == denuncia.DenuncianteId &&
                d.DenunciadoId == denuncia.DenunciadoId &&
                d.Status == denuncia.Status);

            if (jaExiste)
                return BadRequest("Você já fez uma denúncia semelhante.");

            denuncia.DataDenuncia = DateTime.UtcNow;

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();

            return Ok(denuncia);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] DenunciaStatus status)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);

            if (denuncia == null)
                return NotFound();

            denuncia.Status = status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);

            if (denuncia == null)
                return NotFound();

            _context.Denuncias.Remove(denuncia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}