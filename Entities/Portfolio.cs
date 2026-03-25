namespace Fioo.Entities;

public class Portfolio
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string FotoUrl { get; set; } = null!;
    public string? Descricao { get; set; }
    public DateTime DataUpload { get; set; }

    public Usuario Usuario { get; set; } = null!;
}