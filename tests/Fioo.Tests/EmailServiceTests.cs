using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Fioo.Services.Email;
using Fioo.Services.Email.Templates;
using Fioo.Tests.Infra;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Fioo.Tests;

/// <summary>
/// SmtpEmailService contra um servidor SMTP local (STARTTLS + AUTH), sem internet:
/// conexão, autenticação, remetente, HTML, desconexão e tratamento de falhas.
/// O envio real pelo Gmail é testado manualmente com: dotnet run -- --testar-email destinatario@exemplo.com
/// </summary>
public class EmailServiceTests
{
    private const string Usuario = "fioo.sistema@gmail.com";
    private const string Senha = "abcdefghijklmnop";

    private sealed class LoggerMemoria<T> : ILogger<T>
    {
        public ConcurrentQueue<string> Linhas { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Linhas.Enqueue($"{logLevel}: {formatter(state, exception)} {exception}");
    }

    /// <summary>Igual ao serviço real, mas aceita o certificado autoassinado do servidor local.</summary>
    private sealed class ServicoDeTeste(IOptions<EmailSettings> o, ILogger<SmtpEmailService> l) : SmtpEmailService(o, l)
    {
        protected override SmtpClient CriarClienteSmtp()
        {
            var c = base.CriarClienteSmtp();
            c.CheckCertificateRevocation = false;
            c.ServerCertificateValidationCallback = (_, _, _, _) => true;
            return c;
        }
    }

    private static (SmtpEmailService servico, LoggerMemoria<SmtpEmailService> log) Criar(int porta, string? senha, string host = "127.0.0.1")
    {
        var log = new LoggerMemoria<SmtpEmailService>();
        var settings = new EmailSettings { SmtpHost = host, SmtpPort = porta, SmtpPassword = senha, TimeoutSeconds = 5 };
        return (new ServicoDeTeste(Options.Create(settings), log), log);
    }

    [Fact]
    public void Configuracao_padrao_e_a_do_Gmail_e_nao_tem_senha()
    {
        var s = new EmailSettings();
        Assert.Equal("smtp.gmail.com", s.SmtpHost);
        Assert.Equal(587, s.SmtpPort);
        Assert.Equal("fioo.sistema@gmail.com", s.SmtpUsername);
        Assert.Equal("fioo.sistema@gmail.com", s.SenderEmail);
        Assert.Equal("Fioo", s.SenderName);
        Assert.Null(s.SmtpPassword);

        s.SmtpPassword = Senha;
        Assert.DoesNotContain(Senha, s.ToString());
    }

    [Fact]
    public async Task Envia_html_com_starttls_autenticacao_e_remetente_Fioo()
    {
        await using var servidor = new ServidorSmtpLocal(Usuario, Senha);
        // Senha como o Google exibe (em blocos com espaços)
        var (servico, log) = Criar(servidor.Porta, "abcd efgh ijkl mnop");
        var conteudo = EmailTemplates.Teste(DateTime.UtcNow);

        await servico.EnviarAsync("destino@externo.com", conteudo.Assunto, conteudo.Html, conteudo.Texto);

        Assert.True(servidor.UsouStartTls);
        Assert.Equal(Usuario, servidor.UsuarioAutenticado);
        Assert.Contains("<fioo.sistema@gmail.com>", servidor.MailFrom);
        Assert.Contains(servidor.RcptTo, r => r.Contains("<destino@externo.com>"));
        Assert.True(servidor.RecebeuQuit, "O cliente deve encerrar a sessão SMTP (QUIT).");

        var msg = await MimeMessage.LoadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(servidor.Dados!)));
        var de = Assert.Single(msg.From.Mailboxes);
        Assert.Equal("Fioo", de.Name);
        Assert.Equal("fioo.sistema@gmail.com", de.Address);
        Assert.Equal(conteudo.Assunto, msg.Subject);
        Assert.Contains("Teste de envio", msg.HtmlBody);
        Assert.Contains("Teste de envio", msg.TextBody);

        Assert.DoesNotContain(log.Linhas, l => l.Contains(Senha) || l.Contains("abcd efgh"));
    }

    [Fact]
    public async Task Senha_errada_gera_erro_de_autenticacao_sem_vazar_a_senha()
    {
        await using var servidor = new ServidorSmtpLocal(Usuario, Senha);
        var (servico, log) = Criar(servidor.Porta, "senhaerradasecreta");

        var ex = await Assert.ThrowsAsync<EmailEnvioException>(() =>
            servico.EnviarAsync("destino@externo.com", "Assunto", "<p>oi</p>"));

        Assert.Equal(EmailErro.Autenticacao, ex.Erro);
        Assert.True(servidor.RecebeuQuit, "Deve desconectar mesmo após falha de autenticação.");
        Assert.DoesNotContain("senhaerradasecreta", ex.Message);
        Assert.DoesNotContain(log.Linhas, l => l.Contains("senhaerradasecreta"));
        Assert.Contains(log.Linhas, l => l.StartsWith("Error"));
    }

    [Fact]
    public async Task Servidor_inacessivel_gera_erro_de_conexao()
    {
        // Porta livre sem ninguém escutando
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var porta = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        var (servico, _) = Criar(porta, Senha);

        var ex = await Assert.ThrowsAsync<EmailEnvioException>(() =>
            servico.EnviarAsync("destino@externo.com", "Assunto", "<p>oi</p>"));
        Assert.Equal(EmailErro.Conexao, ex.Erro);
    }

    [Fact]
    public async Task Sem_senha_configurada_falha_sem_conectar()
    {
        var (servico, _) = Criar(1, null);

        var ex = await Assert.ThrowsAsync<EmailEnvioException>(() =>
            servico.EnviarAsync("destino@externo.com", "Assunto", "<p>oi</p>"));
        Assert.Equal(EmailErro.Configuracao, ex.Erro);
        Assert.Contains("EmailSettings__SmtpPassword", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public async Task Destinatario_invalido_e_recusado(string destinatario)
    {
        var (servico, _) = Criar(1, Senha);

        var ex = await Assert.ThrowsAsync<EmailEnvioException>(() =>
            servico.EnviarAsync(destinatario, "Assunto", "<p>oi</p>"));
        Assert.Equal(EmailErro.DestinatarioInvalido, ex.Erro);
    }

    [Fact]
    public void Template_de_recuperacao_codifica_dados_dinamicos()
    {
        var c = EmailTemplates.RecuperacaoSenha("<script>x</script>", "https://fioo.app/redefinir-senha?token=a&b", 30);

        Assert.DoesNotContain("<script>", c.Html);
        Assert.Contains("token=a&amp;b", c.Html);
        Assert.Contains("30 minutos", c.Html);
        Assert.Contains("https://fioo.app/redefinir-senha?token=a&b", c.Texto);
    }

    [Fact]
    public void Link_de_redefinicao_vem_da_configuracao_do_frontend()
    {
        var f = new FrontendSettings { BaseUrl = "https://app.fioo.com.br/", RedefinirSenhaPath = "redefinir-senha" };
        Assert.Equal("https://app.fioo.com.br/redefinir-senha?token=abc-_1", f.MontarLinkRedefinicao("abc-_1"));
    }
}
