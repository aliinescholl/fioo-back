public class CriarUsuarioDto
{
    public string? Nome { get; set; }
    public string Email { get; set; }
    public bool EhCostureiro { get; set; } = true;
    public string? CNPJ { get; set; }
    public string Senha { get; set; }
}