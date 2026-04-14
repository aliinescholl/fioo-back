namespace Fioo.Entities;

public class UsuarioMaquinario
{
    public int UsuarioId { get; set; }
    public int MaquinarioId { get; set; }

    public Usuario? Usuario { get; set; }
    public Maquinario? Maquinario { get; set; }
}