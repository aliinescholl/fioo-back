using Fioo.Enums;

public class CriarUsuarioDto
{
    public string Nome { get; set; }
    public string NomeUsuario { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public UsuarioTipo Tipo { get; set; }
}