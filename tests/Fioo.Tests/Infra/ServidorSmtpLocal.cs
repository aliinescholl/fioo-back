using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Fioo.Tests.Infra;

/// <summary>
/// Servidor SMTP mínimo, em memória, que imita o Gmail no que importa ao Fioo:
/// exige STARTTLS antes do AUTH, valida usuário/senha e recebe a mensagem.
/// Permite testar o SmtpEmailService de ponta a ponta sem acessar a internet.
/// </summary>
public sealed class ServidorSmtpLocal : IAsyncDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly X509Certificate2 _certificado = CriarCertificado();
    private readonly string _usuario;
    private readonly string _senha;
    private readonly Task _loop;
    private readonly CancellationTokenSource _cts = new();

    public int Porta { get; }
    public bool UsouStartTls { get; private set; }
    public string? UsuarioAutenticado { get; private set; }
    public string? MailFrom { get; private set; }
    public List<string> RcptTo { get; } = [];
    public string? Dados { get; private set; }
    public bool RecebeuQuit { get; private set; }

    public ServidorSmtpLocal(string usuario, string senha)
    {
        _usuario = usuario;
        _senha = senha;
        _listener.Start();
        Porta = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _loop = Task.Run(AceitarAsync);
    }

    private async Task AceitarAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            TcpClient cliente;
            try { cliente = await _listener.AcceptTcpClientAsync(_cts.Token); }
            catch { return; }
            using (cliente)
            {
                try { await AtenderAsync(cliente.GetStream()); }
                catch (IOException) { }
            }
        }
    }

    private async Task AtenderAsync(Stream stream)
    {
        var (leitor, escritor) = Abrir(stream);
        await escritor.WriteAsync("220 smtp.local ESMTP\r\n");
        var tls = false;
        var autenticado = false;

        while (await leitor.ReadLineAsync() is { } linha)
        {
            var comando = linha.Split(' ', 2)[0].ToUpperInvariant();
            var argumento = linha.Length > comando.Length ? linha[(comando.Length + 1)..] : "";

            switch (comando)
            {
                case "EHLO":
                    await escritor.WriteAsync(tls
                        ? "250-smtp.local\r\n250-AUTH PLAIN LOGIN\r\n250 8BITMIME\r\n"
                        : "250-smtp.local\r\n250-STARTTLS\r\n250 8BITMIME\r\n");
                    break;
                case "STARTTLS":
                    await escritor.WriteAsync("220 2.0.0 Ready to start TLS\r\n");
                    var ssl = new SslStream(stream);
                    await ssl.AuthenticateAsServerAsync(_certificado);
                    stream = ssl;
                    (leitor, escritor) = Abrir(stream);
                    tls = UsouStartTls = true;
                    break;
                case "AUTH":
                    if (!tls) { await escritor.WriteAsync("530 5.7.0 Must issue a STARTTLS command first\r\n"); break; }
                    var (usuario, senha) = await LerCredenciaisAsync(argumento, leitor, escritor);
                    if (usuario == _usuario && senha == _senha)
                    {
                        autenticado = true;
                        UsuarioAutenticado = usuario;
                        await escritor.WriteAsync("235 2.7.0 Accepted\r\n");
                    }
                    else
                    {
                        await escritor.WriteAsync("535 5.7.8 Username and Password not accepted\r\n");
                    }
                    break;
                case "MAIL":
                    if (!autenticado) { await escritor.WriteAsync("530 5.7.0 Authentication Required\r\n"); break; }
                    MailFrom = argumento;
                    await escritor.WriteAsync("250 2.1.0 OK\r\n");
                    break;
                case "RCPT":
                    RcptTo.Add(argumento);
                    await escritor.WriteAsync("250 2.1.5 OK\r\n");
                    break;
                case "DATA":
                    await escritor.WriteAsync("354 Go ahead\r\n");
                    var sb = new StringBuilder();
                    while (await leitor.ReadLineAsync() is { } l && l != ".")
                        sb.Append(l.StartsWith("..") ? l[1..] : l).Append("\r\n");
                    Dados = sb.ToString();
                    await escritor.WriteAsync("250 2.0.0 OK queued\r\n");
                    break;
                case "QUIT":
                    RecebeuQuit = true;
                    await escritor.WriteAsync("221 2.0.0 closing connection\r\n");
                    return;
                default:
                    await escritor.WriteAsync("250 OK\r\n");
                    break;
            }
        }
    }

    private static async Task<(string?, string?)> LerCredenciaisAsync(string argumento, StreamReader leitor, StreamWriter escritor)
    {
        var partes = argumento.Split(' ', 2);
        var mecanismo = partes[0].ToUpperInvariant();
        string Decodificar(string b64) => Encoding.UTF8.GetString(Convert.FromBase64String(b64.Trim()));

        if (mecanismo == "PLAIN")
        {
            var inicial = partes.Length > 1 ? partes[1] : null;
            if (inicial == null)
            {
                await escritor.WriteAsync("334 \r\n");
                inicial = await leitor.ReadLineAsync();
            }
            var campos = Decodificar(inicial!).Split('\0');
            return (campos[1], campos[2]);
        }

        // LOGIN
        string? usuario;
        if (partes.Length > 1) usuario = Decodificar(partes[1]);
        else
        {
            await escritor.WriteAsync("334 VXNlcm5hbWU6\r\n");
            usuario = Decodificar((await leitor.ReadLineAsync())!);
        }
        await escritor.WriteAsync("334 UGFzc3dvcmQ6\r\n");
        var senha = Decodificar((await leitor.ReadLineAsync())!);
        return (usuario, senha);
    }

    private static (StreamReader, StreamWriter) Abrir(Stream s) =>
        (new StreamReader(s, Encoding.ASCII, false, 1024, leaveOpen: true),
         new StreamWriter(s, Encoding.ASCII, 1024, leaveOpen: true) { AutoFlush = true, NewLine = "\r\n" });

    private static X509Certificate2 CriarCertificado()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), null);
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _listener.Stop();
        try { await _loop; } catch { }
        _certificado.Dispose();
    }
}
