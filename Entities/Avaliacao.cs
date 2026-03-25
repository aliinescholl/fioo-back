namespace Fioo.Entities;

public class Avaliacao
{
    public int Id { get; set; }
    public int ServicoId { get; set; }
    public int AvaliadorId { get; set; }
    public int AvaliadoId { get; set; }
    public short Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }

    public Servico Servico { get; set; } = null!;
    public Usuario Avaliador { get; set; } = null!;
    public Usuario Avaliado { get; set; } = null!;
}