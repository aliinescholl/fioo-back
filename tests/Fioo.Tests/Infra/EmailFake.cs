using System.Collections.Concurrent;
using Fioo.Services.Email;

namespace Fioo.Tests.Infra;

/// <summary>Substitui o SMTP nos testes de integração: guarda as mensagens em memória.</summary>
public sealed class EmailFake : IEmailService
{
    public record Mensagem(string Destinatario, string Assunto, string Html, string? Texto);

    public ConcurrentQueue<Mensagem> Enviadas { get; } = new();

    /// <summary>Quando definido, o envio falha com esta exceção.</summary>
    public EmailEnvioException? Falha { get; set; }

    public Task EnviarAsync(string destinatario, string assunto, string html, string? textoAlternativo = null, CancellationToken cancellationToken = default)
    {
        if (Falha != null) throw Falha;
        Enviadas.Enqueue(new Mensagem(destinatario, assunto, html, textoAlternativo));
        return Task.CompletedTask;
    }

    public IReadOnlyList<Mensagem> Para(string destinatario) =>
        Enviadas.Where(m => m.Destinatario == destinatario).ToList();
}
