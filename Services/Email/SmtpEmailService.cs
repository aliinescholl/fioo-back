using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Fioo.Services.Email;

/// <summary>
/// Envio via SMTP do Gmail (smtp.gmail.com:587, STARTTLS) com senha de aplicativo.
/// A senha nunca é registrada em log nem incluída nas mensagens de erro.
/// </summary>
public class SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task EnviarAsync(string destinatario, string assunto, string html, string? textoAlternativo = null, CancellationToken cancellationToken = default)
    {
        ValidarConfiguracao();

        if (!MailboxAddress.TryParse(destinatario?.Trim() ?? "", out var para) || string.IsNullOrEmpty(para.Address) || !para.Address.Contains('@'))
            throw new EmailEnvioException(EmailErro.DestinatarioInvalido, "Endereço de e-mail do destinatário inválido.");

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        mensagem.To.Add(para);
        mensagem.Subject = assunto;
        mensagem.Body = new BodyBuilder { HtmlBody = html, TextBody = textoAlternativo }.ToMessageBody();

        var destinoLog = MascararEmail(para.Address);
        using var smtp = CriarClienteSmtp();

        try
        {
            try
            {
                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            }
            catch (Exception ex) when (ex is SocketException or IOException or SslHandshakeException or ProtocolException or TimeoutException or NotSupportedException)
            {
                logger.LogError(ex, "Falha ao conectar ao servidor SMTP {Host}:{Porta}.", _settings.SmtpHost, _settings.SmtpPort);
                throw new EmailEnvioException(EmailErro.Conexao, $"Não foi possível conectar ao servidor de e-mail ({_settings.SmtpHost}:{_settings.SmtpPort}).", ex);
            }

            try
            {
                await smtp.AuthenticateAsync(_settings.SmtpUsername, SenhaNormalizada(), cancellationToken);
            }
            catch (AuthenticationException ex)
            {
                // Sem a exceção completa: a mensagem do servidor é suficiente e não contém a senha
                logger.LogError("Falha de autenticação SMTP para {Usuario}: {Motivo}", _settings.SmtpUsername, ex.Message);
                throw new EmailEnvioException(EmailErro.Autenticacao, "Falha de autenticação no servidor de e-mail. Verifique a senha de aplicativo configurada.", ex);
            }

            try
            {
                await smtp.SendAsync(mensagem, cancellationToken);
            }
            catch (SmtpCommandException ex)
            {
                logger.LogError("Servidor SMTP recusou o envio para {Destino}: {Status} {Motivo}", destinoLog, ex.StatusCode, ex.Message);
                var erro = ex.ErrorCode == SmtpErrorCode.RecipientNotAccepted ? EmailErro.DestinatarioInvalido : EmailErro.Envio;
                throw new EmailEnvioException(erro, "O servidor de e-mail recusou a mensagem.", ex);
            }

            logger.LogInformation("E-mail \"{Assunto}\" enviado para {Destino}.", assunto, destinoLog);
        }
        catch (EmailEnvioException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is SmtpProtocolException or IOException or SocketException or TimeoutException)
        {
            logger.LogError(ex, "Conexão SMTP interrompida ao enviar para {Destino}.", destinoLog);
            throw new EmailEnvioException(EmailErro.Conexao, "A conexão com o servidor de e-mail foi interrompida.", ex);
        }
        finally
        {
            if (smtp.IsConnected)
            {
                try
                {
                    await smtp.DisconnectAsync(true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Erro ao desconectar do servidor SMTP: {Motivo}", ex.Message);
                }
            }
        }
    }

    /// <summary>Ponto de extensão para testes (ex.: aceitar certificado de um servidor SMTP local).</summary>
    protected virtual SmtpClient CriarClienteSmtp() => new() { Timeout = _settings.TimeoutSeconds * 1000 };

    private void ValidarConfiguracao()
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpPassword))
        {
            logger.LogError("Senha de aplicativo SMTP não configurada (variável de ambiente EmailSettings__SmtpPassword).");
            throw new EmailEnvioException(EmailErro.Configuracao, "Envio de e-mail não configurado: defina a variável de ambiente EmailSettings__SmtpPassword.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || _settings.SmtpPort <= 0 ||
            string.IsNullOrWhiteSpace(_settings.SmtpUsername) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            throw new EmailEnvioException(EmailErro.Configuracao, "Configuração SMTP incompleta (EmailSettings).");
        }
    }

    // O Google exibe a senha de aplicativo em blocos ("abcd efgh ijkl mnop"); os espaços não fazem parte dela
    private string SenhaNormalizada() => _settings.SmtpPassword!.Replace(" ", "").Trim();

    /// <summary>"maria.silva@x.com" → "ma***@x.com" (evita dados pessoais completos no log).</summary>
    internal static string MascararEmail(string email)
    {
        var arroba = email.IndexOf('@');
        if (arroba <= 0) return "***";
        var local = email[..arroba];
        return $"{local[..Math.Min(2, local.Length)]}***{email[arroba..]}";
    }
}
