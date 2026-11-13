namespace Fioo.Services.Email;

/// <summary>
/// Configuração do envio de e-mails (seção "EmailSettings").
/// Os valores não secretos têm padrão aqui e em appsettings.json.
/// A senha de aplicativo do Google vem SOMENTE da variável de ambiente
/// EmailSettings__SmtpPassword (ou user-secrets em desenvolvimento) — nunca do código.
/// </summary>
public class EmailSettings
{
    public const string Secao = "EmailSettings";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "fioo.sistema@gmail.com";
    public string SenderEmail { get; set; } = "fioo.sistema@gmail.com";
    public string SenderName { get; set; } = "Fioo";

    /// <summary>Tempo máximo de cada operação SMTP (conexão, autenticação, envio).</summary>
    public int TimeoutSeconds { get; set; } = 20;

    /// <summary>Senha de aplicativo do Google. Nunca registrar em log.</summary>
    public string? SmtpPassword { get; set; }

    // Evita que a senha apareça se o objeto for serializado/logado por engano
    public override string ToString() =>
        $"{SmtpHost}:{SmtpPort} (STARTTLS) usuário={SmtpUsername} remetente=\"{SenderName}\" <{SenderEmail}> senha={(string.IsNullOrWhiteSpace(SmtpPassword) ? "não configurada" : "***")}";
}
