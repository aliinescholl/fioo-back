namespace Fioo.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// Envia uma mensagem HTML a partir do remetente oficial do Fioo.
    /// </summary>
    /// <param name="destinatario">E-mail do destinatário.</param>
    /// <param name="assunto">Assunto da mensagem.</param>
    /// <param name="html">Corpo em HTML.</param>
    /// <param name="textoAlternativo">Versão em texto puro (opcional, para clientes sem HTML).</param>
    /// <exception cref="EmailEnvioException">Falha de configuração, conexão, autenticação ou envio.</exception>
    Task EnviarAsync(string destinatario, string assunto, string html, string? textoAlternativo = null, CancellationToken cancellationToken = default);
}

public enum EmailErro
{
    Configuracao,
    DestinatarioInvalido,
    Conexao,
    Autenticacao,
    Envio
}

public class EmailEnvioException : Exception
{
    public EmailErro Erro { get; }

    public EmailEnvioException(EmailErro erro, string message, Exception? inner = null) : base(message, inner)
    {
        Erro = erro;
    }
}
