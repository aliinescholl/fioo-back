namespace Fioo.Entities;

public class Verificacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public bool EtapaEmail { get; set; }
    public bool EtapaDocumento { get; set; }
    public bool EtapaTempo { get; set; }
    public string? FotoSelfieUrl { get; set; }
    public DateTime? DataEnvioDocumento { get; set; }
    public DateTime? DataVerificacaoCompleta { get; set; }

    public Usuario Usuario { get; set; }
}