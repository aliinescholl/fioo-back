using Fioo.Data;
using Fioo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Controllers;

[ApiController]
[Route("api/maquinarios")]
public class MaquinariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaquinariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] Maquinario maquinario)
    {
        _context.Maquinarios.Add(maquinario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = maquinario.Id }, maquinario);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _context.Maquinarios.ToListAsync();
        return Ok(lista);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var maquinario = await _context.Maquinarios.FindAsync(id);

        if (maquinario == null)
            return NotFound();

        return Ok(maquinario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var maquinario = await _context.Maquinarios.FindAsync(id);

        if (maquinario == null)
            return NotFound();

        _context.Maquinarios.Remove(maquinario);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}