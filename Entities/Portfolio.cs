namespace Fioo.Entities;

public class Portfolio : EntidadeBase
{
    public int UsuarioId { get; set; }
    public string FotoUrl { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataUpload { get; set; }

    public Usuario? Usuario { get; set; }
}