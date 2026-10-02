using Fioo.Enums;

namespace Fioo.DTOs;

public class CriarAvaliacaoDto
{
    public int ServicoId { get; set; }
    // Nota geral
    public int Nota { get; set; }
    public int NotaComunicacao { get; set; }
    public int NotaQualidade { get; set; }
    public string? Comentario { get; set; }
}

public class AvaliacaoDto
{
    public int Id { get; set; }
    public int ServicoId { get; set; }
    public string ServicoTitulo { get; set; } = null!;
    public UsuarioResumoDto Avaliador { get; set; } = null!;
    public int AvaliadoId { get; set; }
    public UsuarioTipo PapelAvaliado { get; set; }
    public int Nota { get; set; }
    public int NotaComunicacao { get; set; }
    public int NotaQualidade { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }
}

public class ResumoAvaliacoesDto
{
    // Média da nota geral (null quando não há avaliações)
    public double? Media { get; set; }
    public int Total { get; set; }
}

public class AvaliacoesUsuarioDto
{
    public ResumoAvaliacoesDto ComoCostureiro { get; set; } = new();
    public ResumoAvaliacoesDto ComoFornecedor { get; set; } = new();
    public List<AvaliacaoDto> Itens { get; set; } = [];
}
