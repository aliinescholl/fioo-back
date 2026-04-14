namespace Fioo.Entities;
using Fioo.Enums;

public class Denuncia
{
    public int Id { get; set; }
    public int DenuncianteId { get; set; }
    public int DenunciadoId { get; set; }
    public string Motivo { get; set; }
    public string? Descricao { get; set; }
    public DenunciaStatus Status { get; set; }
    public DateTime DataDenuncia { get; set; }

    public Usuario? Denunciante { get; set; }
    public Usuario? Denunciado { get; set; }
}