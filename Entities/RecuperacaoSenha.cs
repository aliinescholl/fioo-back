namespace Fioo.Entities;

/// <summary>
/// Solicitação de recuperação de senha. Guarda apenas o hash SHA-256 do token;
/// o token em si só existe no link enviado por e-mail.
/// </summary>
public class RecuperacaoSenha
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTime DataCriacao { get; set; }
    public DateTime DataExpiracao { get; set; }
    public DateTime? DataUso { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
