using System.Net;

namespace Fioo.Services.Email.Templates;

/// <summary>
/// Templates HTML dos e-mails do Fioo. Layout em tabelas e estilos inline para
/// funcionar no Gmail, Outlook e clientes móveis. Todo valor dinâmico é codificado.
/// </summary>
public static class EmailTemplates
{
    // Identidade visual (mesmas cores do frontend: globals.css)
    private const string CorPrimaria = "#84c4b4";
    private const string CorEscura = "#2a594d";
    private const string CorFundo = "#eef6f4";
    private const string CorTexto = "#2f3b38";
    private const string CorSuave = "#6b7f7a";

    public record Conteudo(string Assunto, string Html, string Texto);

    public static Conteudo RecuperacaoSenha(string? nome, string link, int validadeMinutos)
    {
        var saudacao = string.IsNullOrWhiteSpace(nome) ? "Olá!" : $"Olá, {WebUtility.HtmlEncode(nome.Trim())}!";
        var linkHtml = WebUtility.HtmlEncode(link);

        var corpo = $"""
            <p style="margin:0 0 16px;font-size:18px;font-weight:bold;color:{CorEscura};">{saudacao}</p>
            <p style="margin:0 0 16px;">Recebemos uma solicitação para redefinir a senha da sua conta no <strong>Fioo</strong>.</p>
            <p style="margin:0 0 24px;">Para criar uma nova senha, clique no botão abaixo:</p>
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" align="center" style="margin:0 auto 24px;">
              <tr>
                <td align="center" bgcolor="{CorEscura}" style="border-radius:8px;">
                  <a href="{linkHtml}" target="_blank" style="display:inline-block;padding:14px 32px;font-size:16px;font-weight:bold;color:#ffffff;text-decoration:none;border-radius:8px;">Redefinir senha</a>
                </td>
              </tr>
            </table>
            <p style="margin:0 0 8px;font-size:14px;color:{CorSuave};">Se o botão não funcionar, copie e cole este link no navegador:</p>
            <p style="margin:0 0 24px;font-size:13px;word-break:break-all;"><a href="{linkHtml}" target="_blank" style="color:{CorEscura};">{linkHtml}</a></p>
            <p style="margin:0 0 16px;padding:12px 16px;background:{CorFundo};border-left:4px solid {CorPrimaria};border-radius:4px;font-size:14px;">
              Este link é válido por <strong>{validadeMinutos} minutos</strong> e só pode ser usado uma vez.
            </p>
            <p style="margin:0;font-size:14px;color:{CorSuave};">Se você não solicitou a recuperação de senha, ignore esta mensagem. Sua senha atual continua a mesma.</p>
            """;

        var texto = $"""
            {(string.IsNullOrWhiteSpace(nome) ? "Olá!" : $"Olá, {nome.Trim()}!")}

            Recebemos uma solicitação para redefinir a senha da sua conta no Fioo.

            Para criar uma nova senha, acesse o link abaixo:
            {link}

            Este link é válido por {validadeMinutos} minutos e só pode ser usado uma vez.

            Se você não solicitou a recuperação de senha, ignore esta mensagem. Sua senha atual continua a mesma.

            — Equipe Fioo
            """;

        const string assunto = "Fioo – Recuperação de senha";
        return new Conteudo(assunto, Layout(assunto, "Redefina a senha da sua conta no Fioo.", corpo), texto);
    }

    public static Conteudo Teste(DateTime enviadoEmUtc)
    {
        var corpo = $"""
            <p style="margin:0 0 16px;font-size:18px;font-weight:bold;color:{CorEscura};">Teste de envio</p>
            <p style="margin:0 0 16px;">Se você está lendo esta mensagem, a integração SMTP do <strong>Fioo</strong> está funcionando.</p>
            <ul style="margin:0 0 16px;padding-left:20px;font-size:14px;">
              <li>Conexão SMTP com STARTTLS: ok</li>
              <li>Autenticação: ok</li>
              <li>Formatação HTML: <span style="color:{CorEscura};font-weight:bold;">ok</span></li>
            </ul>
            <p style="margin:0;font-size:13px;color:{CorSuave};">Enviado em {enviadoEmUtc:dd/MM/yyyy HH:mm:ss} (UTC).</p>
            """;
        const string assunto = "Fioo – Teste de envio de e-mail";
        var texto = $"Teste de envio do Fioo. Enviado em {enviadoEmUtc:dd/MM/yyyy HH:mm:ss} (UTC).";
        return new Conteudo(assunto, Layout(assunto, "Teste da integração de e-mail do Fioo.", corpo), texto);
    }

    private static string Layout(string titulo, string preheader, string corpo) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>{WebUtility.HtmlEncode(titulo)}</title>
        </head>
        <body style="margin:0;padding:0;background:{CorFundo};">
          <div style="display:none;max-height:0;overflow:hidden;opacity:0;">{WebUtility.HtmlEncode(preheader)}</div>
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background:{CorFundo};">
            <tr>
              <td align="center" style="padding:32px 16px;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:560px;background:#ffffff;border-radius:12px;overflow:hidden;font-family:Georgia,'Times New Roman',serif;color:{CorTexto};font-size:16px;line-height:1.5;">
                  <tr>
                    <td align="center" bgcolor="{CorPrimaria}" style="padding:28px 24px;">
                      <span style="font-size:34px;font-weight:bold;letter-spacing:1px;color:{CorEscura};">Fioo</span>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:32px 32px 24px;">
                      {corpo}
                    </td>
                  </tr>
                  <tr>
                    <td align="center" style="padding:16px 24px 28px;border-top:1px solid {CorFundo};font-size:12px;color:{CorSuave};">
                      Equipe Fioo · Esta é uma mensagem automática, não é necessário respondê-la.
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
}
