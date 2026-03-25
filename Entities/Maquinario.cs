namespace Fioo.Entities;

public class Maquinario
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;

    public ICollection<UsuarioMaquinario> Usuarios { get; set; } = [];
    public ICollection<ServicoMaquinario> Servicos { get; set; } = [];
}