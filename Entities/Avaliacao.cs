namespace Fioo.Entities;

public class Avaliacao : EntidadeBase
{
    public int ServicoId { get; set; }
    public int AvaliadorId { get; set; }
    public int AvaliadoId { get; set; }
    public short Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }

    public Servico? Servico { get; set; }
    public Usuario? Avaliador { get; set; }
    public Usuario? Avaliado { get; set; }
}