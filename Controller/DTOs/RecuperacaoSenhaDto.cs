namespace Fioo.Controller.DTOs;

public class SolicitarRecuperacaoSenhaDto
{
    public string? Email { get; set; }
}

public class RedefinirSenhaDto
{
    public string? Token { get; set; }
    public string? NovaSenha { get; set; }
}
