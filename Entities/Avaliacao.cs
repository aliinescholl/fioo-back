namespace Fioo.Entities;
using Fioo.Enums;

/// <summary>
/// Avaliação de um participante de um serviço concluído pelo outro participante
/// (costureiro avalia fornecedor e fornecedor avalia costureiro).
/// </summary>
public class Avaliacao : EntidadeBase
{
    public int ServicoId { get; set; }
    public int AvaliadorId { get; set; }
    public int AvaliadoId { get; set; }

    // Papel do avaliado neste serviço: Fornecedor (dono do serviço) ou Costureiro (vinculado)
    public UsuarioTipo PapelAvaliado { get; set; }

    // Nota geral (1 a 5)
    public short Nota { get; set; }
    public short NotaComunicacao { get; set; }
    public short NotaQualidade { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }

    public Servico? Servico { get; set; }
    public Usuario? Avaliador { get; set; }
    public Usuario? Avaliado { get; set; }
}
