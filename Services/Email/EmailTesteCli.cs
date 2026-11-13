using Fioo.Services.Email.Templates;
using Microsoft.Extensions.Options;

namespace Fioo.Services.Email;

/// <summary>
/// Teste manual do SMTP pela linha de comando, sem endpoint HTTP:
///   dotnet run -- --testar-email destinatario@exemplo.com
/// Envia uma mensagem HTML de teste usando exatamente a mesma configuração da API e encerra.
/// </summary>
public static class EmailTesteCli
{
    public const string Argumento = "--testar-email";

    public static bool FoiSolicitado(string[] args) => args.Contains(Argumento);

    public static async Task<int> ExecutarAsync(IServiceProvider services, string[] args)
    {
        var indice = Array.IndexOf(args, Argumento);
        var destinatario = indice + 1 < args.Length ? args[indice + 1] : null;
        if (string.IsNullOrWhiteSpace(destinatario) || destinatario.StartsWith("--"))
        {
            Console.Error.WriteLine($"Uso: dotnet run -- {Argumento} destinatario@exemplo.com");
            return 2;
        }

        using var scope = services.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        Console.WriteLine($"Configuração: {settings}"); // ToString() nunca mostra a senha
        Console.WriteLine($"Enviando e-mail de teste para {destinatario}...");

        var conteudo = EmailTemplates.Teste(DateTime.UtcNow);
        try
        {
            await email.EnviarAsync(destinatario, conteudo.Assunto, conteudo.Html, conteudo.Texto);
            Console.WriteLine($"OK: mensagem enviada por \"{settings.SenderName}\" <{settings.SenderEmail}>. Confira a caixa de entrada (e o spam) do destinatário.");
            return 0;
        }
        catch (EmailEnvioException ex)
        {
            Console.Error.WriteLine($"FALHA ({ex.Erro}): {ex.Message}");
            return 1;
        }
    }
}
