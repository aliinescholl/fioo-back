namespace Fioo.Entities;

public class Maquinario : EntidadeBase
{
    public string Nome { get; set; }

    public ICollection<UsuarioMaquinario>? Usuarios { get; set; }
    public ICollection<ServicoMaquinario>? Servicos { get; set; }
}