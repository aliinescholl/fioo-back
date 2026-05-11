using Fioo.Data;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<IEnumerable<Servico>>> GetAll()
        {
            // Fornecedores e Costureiros podem visualizar
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

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Servico>>> GetByUsuario(int usuarioId)
        {
            return await _context.Servicos
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.Usuario)
                .ToListAsync();
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
                DataPrazo = DateOnly.Parse(dto.DataPrazo),
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