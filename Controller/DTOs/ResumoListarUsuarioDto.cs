namespace Fioo.Controller.DTOs;

public class ResumoListarUsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Localizacao { get; set; }
    public string? Foto { get; set; }
    public string? FotoUrl => Foto;
    public string MediaEstrela { get; set; } = "0";
}
