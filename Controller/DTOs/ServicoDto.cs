using Fioo.Enums;
using System;

public class ServicoDto
{
    public int UsuarioId { get; set; } // id do dono (deve bater com token)
    public string Titulo { get; set; } = null!;
    public string? Descricao { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public CobrancaTipo TipoCobranca { get; set; }
    public string? CategoriaServico { get; set; }
    public decimal? Valor { get; set; }
    public PrazoTipo? TipoPrazo { get; set; }
    public string? DataPrazo { get; set; }
    public ServicoStatus Status { get; set; }
}