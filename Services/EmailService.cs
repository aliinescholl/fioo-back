using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Fioo.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public virtual async Task EnviarAsync(string para, string assunto, string html)
    {
        var senhaApp = _config["Email:SenhaApp"];
        var host = _config["Email:Host"] ?? "smtp.gmail.com";
        var porta = int.TryParse(_config["Email:Porta"], out var p) ? p : 587;
        var usuario = _config["Email:Usuario"] ?? "fioo.sistema@gmail.com";
        var nomeRemetente = _config["Email:NomeRemetente"] ?? "Fioo";

        if (string.IsNullOrWhiteSpace(senhaApp))
        {
            _logger.LogWarning("Email:SenhaApp não configurada no User Secrets / Variáveis de Ambiente. E-mail para '{Para}' ('{Assunto}') não pôde ser enviado via SMTP.", para, assunto);
            return;
        }

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(nomeRemetente, usuario));
        msg.To.Add(MailboxAddress.Parse(para));
        msg.Subject = assunto;
        msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, porta, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(usuario, senhaApp);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }
}
