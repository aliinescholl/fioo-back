using Fioo.DTOs;

namespace Fioo.Controller.DTOs;

/// <summary>
/// Filtros e ordenação da listagem de serviços (query string de GET /api/servicos).
/// Todos os filtros são opcionais e combináveis.
/// </summary>
public class ServicoFiltroDto
{
    public string? Busca { get; set; }
    public string? Uf { get; set; }
    public string? Cidade { get; set; }
    public decimal? ValorMin { get; set; }
    public decimal? ValorMax { get; set; }
    public int? Cobranca { get; set; }
    public int? Prazo { get; set; }
    public string? Categoria { get; set; }
    public int? Status { get; set; }

    /// <summary>relevantes (padrão), prazo-proximo, prazo-distante, maior-valor, menor-valor</summary>
    public string? Ordenacao { get; set; }

    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}

public class PaginaServicosDto
{
    public List<ServicoResumoDto> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public bool TemMais { get; set; }
}
