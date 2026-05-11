using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fioo.Entities;
using Fioo.Data;
using Fioo.DTOs;
using Fioo.Enums;

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
        public async Task<ActionResult<IEnumerable<ServicoResumoDto>>> GetAll([FromQuery] int usuarioId)
        {
            var servicos = await _context.Servicos
                .Where(s => s.Status == ServicoStatus.Ativo && s.UsuarioId != usuarioId)
                .Include(s => s.Usuario)
                .Include(s => s.Maquinarios)!
                    .ThenInclude(sm => sm.Maquinario)
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
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null)
                return NotFound();

            return Ok(ToResumoDto(servico));
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
            Usuario = new UsuarioResumoDto
            {
                Id = s.Usuario!.Id,
                Nome = s.Usuario.Nome,
                NomeUsuario = s.Usuario.NomeUsuario,
                FotoPerfilUrl = s.Usuario.FotoPerfilUrl,
                Cidade = s.Usuario.Cidade,
                Estado = s.Usuario.Estado
            },
            Maquinarios = s.Maquinarios?
                .Where(sm => sm.Maquinario != null)
                .Select(sm => new MaquinarioResumoDto
                {
                    Id = sm.Maquinario!.Id,
                    Nome = sm.Maquinario.Nome
                }).ToList() ?? []
        };
    }
}