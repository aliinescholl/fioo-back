using Fioo.Data;
using Fioo.Data.Configurations;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Fioo.Controllers
{
    /// <summary>
    /// Avaliação mútua entre o fornecedor dono de um serviço concluído e o costureiro vinculado.
    /// Cada participante avalia apenas a outra parte, uma única vez (sem edição nem exclusão).
    /// </summary>
    [ApiController]
    [Route("api/avaliacoes")]
    [Authorize]
    public class AvaliacoesController : ControllerBase
    {
        private const int LimiteListagem = 50;

        private readonly AppDbContext _context;

        public AvaliacoesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<AvaliacaoDto>> Create([FromBody] CriarAvaliacaoDto dto)
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            foreach (var (campo, rotulo, valor) in new[]
            {
                ("nota", "A nota geral", dto.Nota),
                ("notaComunicacao", "A nota de comunicação", dto.NotaComunicacao),
                ("notaQualidade", "A nota de qualidade do serviço", dto.NotaQualidade)
            })
            {
                if (valor is < 1 or > 5)
                    return BadRequest(new { field = campo, message = $"{rotulo} deve ser de 1 a 5 estrelas." });
            }

            var comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim();
            if (comentario?.Length > AvaliacaoConfiguration.ComentarioMaxLength)
                return BadRequest(new { field = "comentario", message = $"O comentário pode ter no máximo {AvaliacaoConfiguration.ComentarioMaxLength} caracteres." });

            var servico = await _context.Servicos
                .Where(s => s.Id == dto.ServicoId)
                .Select(s => new
                {
                    s.UsuarioId,
                    s.Status,
                    CostureiroId = s.Candidaturas!
                        .Where(c => c.Status == CandidaturaStatus.Aceita)
                        .Select(c => (int?)c.UsuarioId)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (servico == null)
                return NotFound(new { message = "Serviço não encontrado." });

            if (servico.Status != ServicoStatus.Concluido)
                return Conflict(new { message = "Só é possível avaliar serviços concluídos." });

            // Quem avalia é sempre o usuário do token; o avaliado é a outra parte do serviço
            int avaliadoId;
            UsuarioTipo papelAvaliado;
            if (userId.Value == servico.UsuarioId && servico.CostureiroId != null)
            {
                avaliadoId = servico.CostureiroId.Value;
                papelAvaliado = UsuarioTipo.Costureiro;
            }
            else if (userId.Value == servico.CostureiroId)
            {
                avaliadoId = servico.UsuarioId;
                papelAvaliado = UsuarioTipo.Fornecedor;
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Só o fornecedor e o costureiro deste serviço podem avaliá-lo." });
            }

            var jaAvaliou = await _context.Avaliacoes
                .AnyAsync(a => a.ServicoId == dto.ServicoId && a.AvaliadorId == userId.Value);
            if (jaAvaliou)
                return Conflict(new { message = "Você já avaliou este serviço." });

            var avaliacao = new Avaliacao
            {
                ServicoId = dto.ServicoId,
                AvaliadorId = userId.Value,
                AvaliadoId = avaliadoId,
                PapelAvaliado = papelAvaliado,
                Nota = (short)dto.Nota,
                NotaComunicacao = (short)dto.NotaComunicacao,
                NotaQualidade = (short)dto.NotaQualidade,
                Comentario = comentario,
                DataAvaliacao = DateTime.UtcNow,
                Ativo = true
            };

            _context.Avaliacoes.Add(avaliacao);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Duas avaliações enviadas ao mesmo tempo (índice único ServicoId + AvaliadorId)
                return Conflict(new { message = "Você já avaliou este serviço." });
            }

            var criada = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Id == avaliacao.Id)
                .Select(ToDto)
                .FirstAsync();

            return CreatedAtAction(nameof(GetByServico), new { servicoId = dto.ServicoId }, criada);
        }

        /// <summary>Ids dos serviços que o usuário autenticado já avaliou.</summary>
        [HttpGet("feitas")]
        public async Task<ActionResult<IEnumerable<int>>> GetFeitas()
        {
            var userId = GetUsuarioIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var servicoIds = await _context.Avaliacoes
                .Where(a => a.AvaliadorId == userId.Value)
                .Select(a => a.ServicoId)
                .ToListAsync();

            return Ok(servicoIds);
        }

        /// <summary>Avaliações de um serviço (até duas: uma de cada participante).</summary>
        [HttpGet("servico/{servicoId}")]
        public async Task<ActionResult<IEnumerable<AvaliacaoDto>>> GetByServico(int servicoId)
        {
            if (!await _context.Servicos.AnyAsync(s => s.Id == servicoId))
                return NotFound(new { message = "Serviço não encontrado." });

            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.ServicoId == servicoId)
                .OrderBy(a => a.DataAvaliacao)
                .ThenBy(a => a.Id)
                .Select(ToDto)
                .ToListAsync();

            return Ok(avaliacoes);
        }

        /// <summary>
        /// Avaliações recebidas por um usuário (mais recentes primeiro) com média e total
        /// separados por papel. Opcionalmente filtradas por papel (?papel=0 costureiro, 1 fornecedor).
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<AvaliacoesUsuarioDto>> GetByUsuario(int usuarioId, [FromQuery] int? papel)
        {
            if (papel.HasValue && !Enum.IsDefined(typeof(UsuarioTipo), papel.Value))
                return BadRequest(new { field = "papel", message = "Papel inválido." });

            if (!await _context.Usuarios.AnyAsync(u => u.Id == usuarioId))
                return NotFound(new { message = "Usuário não encontrado." });

            var recebidas = _context.Avaliacoes.AsNoTracking().Where(a => a.AvaliadoId == usuarioId);

            var resumos = await recebidas
                .GroupBy(a => a.PapelAvaliado)
                .Select(g => new { Papel = g.Key, Media = g.Average(a => (double)a.Nota), Total = g.Count() })
                .ToListAsync();

            ResumoAvaliacoesDto Resumo(UsuarioTipo p)
            {
                var r = resumos.FirstOrDefault(x => x.Papel == p);
                return r == null
                    ? new ResumoAvaliacoesDto()
                    : new ResumoAvaliacoesDto { Media = Math.Round(r.Media, 1), Total = r.Total };
            }

            if (papel.HasValue)
            {
                var papelFiltro = (UsuarioTipo)papel.Value;
                recebidas = recebidas.Where(a => a.PapelAvaliado == papelFiltro);
            }

            var itens = await recebidas
                .OrderByDescending(a => a.DataAvaliacao)
                .ThenByDescending(a => a.Id)
                .Take(LimiteListagem)
                .Select(ToDto)
                .ToListAsync();

            return Ok(new AvaliacoesUsuarioDto
            {
                ComoCostureiro = Resumo(UsuarioTipo.Costureiro),
                ComoFornecedor = Resumo(UsuarioTipo.Fornecedor),
                Itens = itens
            });
        }

        private static readonly Expression<Func<Avaliacao, AvaliacaoDto>> ToDto = a => new AvaliacaoDto
        {
            Id = a.Id,
            ServicoId = a.ServicoId,
            ServicoTitulo = a.Servico!.Titulo,
            Avaliador = new UsuarioResumoDto
            {
                Id = a.Avaliador!.Id,
                Nome = a.Avaliador.Nome,
                NomeUsuario = a.Avaliador.NomeUsuario,
                FotoPerfilUrl = a.Avaliador.FotoPerfilUrl,
                Cidade = a.Avaliador.Cidade,
                Estado = a.Avaliador.Estado
            },
            AvaliadoId = a.AvaliadoId,
            PapelAvaliado = a.PapelAvaliado,
            Nota = a.Nota,
            NotaComunicacao = a.NotaComunicacao,
            NotaQualidade = a.NotaQualidade,
            Comentario = a.Comentario,
            DataAvaliacao = a.DataAvaliacao
        };

        private int? GetUsuarioIdFromClaims()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
