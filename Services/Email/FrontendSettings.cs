namespace Fioo.Services.Email;

/// <summary>
/// Endereços do frontend usados em links enviados por e-mail (seção "Frontend").
/// Em produção, sobrescrever com a variável de ambiente Frontend__BaseUrl.
/// </summary>
public class FrontendSettings
{
    public const string Secao = "Frontend";

    public string BaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>Rota da página de redefinição de senha; recebe o token em ?token=.</summary>
    public string RedefinirSenhaPath { get; set; } = "/redefinir-senha";

    public string MontarLinkRedefinicao(string token) =>
        $"{BaseUrl.TrimEnd('/')}/{RedefinirSenhaPath.TrimStart('/')}?token={Uri.EscapeDataString(token)}";
}
