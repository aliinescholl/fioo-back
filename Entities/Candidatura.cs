namespace Fioo.Entities;
using Fioo.Enums;

public class Candidatura : EntidadeBase
{
    public int ServicoId { get; set; }
    public int UsuarioId { get; set; }
    public CandidaturaStatus Status { get; set; }
    public DateTime DataCandidatura { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public Servico? Servico { get; set; } = null!;
    public Usuario? Usuario { get; set; } = null!;
}