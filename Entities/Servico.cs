namespace Fioo.Entities;
using Fioo.Enums;

public class Servico
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public CobrancaTipo TipoCobranca { get; set; }
    public string? CategoriaServico { get; set; }
    public decimal? Valor { get; set; }
    public PrazoTipo? TipoPrazo { get; set; }
    public DateOnly? DataPrazo { get; set; }
    public ServicoStatus Status { get; set; }
    public DateTime DataCriacao { get; set; }

    public Usuario? Usuario { get; set; }
    public List<Candidatura>? Candidaturas { get; set; }
    public List<Avaliacao>? Avaliacoes { get; set; }
    public List<ServicoMaquinario>? Maquinarios { get; set; }
}