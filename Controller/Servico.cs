using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fioo.Entities;
using Fioo.Data;

namespace Fioo.Controllers
{
    [ApiController]
    [Route("api/servicos")]
    public class ServicosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servico>>> GetAll()
        {
            return await _context.Servicos
                .Include(s => s.Usuario)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Servico>> GetById(int id)
        {
            var servico = await _context.Servicos
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null)
                return NotFound();

            return servico;
        }

        [HttpPost]
        public async Task<ActionResult<Servico>> Create(Servico servico)
        {
            servico.DataCriacao = DateTime.UtcNow;

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

            // Atualizando campos
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

            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}