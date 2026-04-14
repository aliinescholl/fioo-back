using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fioo.Data;
using Fioo.Entities;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/portfolios")]
    public class PortfoliosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PortfoliosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Portfolio>>> GetAll()
        {
            return await _context.Portfolios
                .Include(p => p.Usuario)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Portfolio>> GetById(int id)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (portfolio == null)
                return NotFound();

            return portfolio;
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Portfolio>>> GetByUsuario(int usuarioId)
        {
            return await _context.Portfolios
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Portfolio>> Create(Portfolio portfolio)
        {
            portfolio.DataUpload = DateTime.UtcNow;

            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = portfolio.Id }, portfolio);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Portfolio portfolio)
        {
            if (id != portfolio.Id)
                return BadRequest();

            var existing = await _context.Portfolios.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.FotoUrl = portfolio.FotoUrl;
            existing.Descricao = portfolio.Descricao;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);

            if (portfolio == null)
                return NotFound();

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}