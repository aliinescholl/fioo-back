namespace Fioo.Entities;

public class ServicoMaquinario
{
    public int ServicoId { get; set; }
    public int MaquinarioId { get; set; }

    public Servico Servico { get; set; } = null!;
    public Maquinario Maquinario { get; set; } = null!;
}