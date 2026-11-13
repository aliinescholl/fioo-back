namespace Fioo.Services.Email;

/// <summary>
/// Endereços do frontend usados em links enviados por e-mail.
/// BaseUrl vem da configuração "FrontendUrl" (a mesma do CORS); em produção, variável de ambiente FrontendUrl.
/// </summary>
public class FrontendSettings
{
    public string BaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>Rota da página de redefinição de senha; recebe o token em ?token=.</summary>
    public string RedefinirSenhaPath { get; set; } = "/redefinir-senha";

    public string MontarLinkRedefinicao(string token) =>
        $"{BaseUrl.TrimEnd('/')}/{RedefinirSenhaPath.TrimStart('/')}?token={Uri.EscapeDataString(token)}";
}
