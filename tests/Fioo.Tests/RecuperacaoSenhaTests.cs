using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Services.Email;
using Fioo.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fluxo de recuperação de senha: solicitação → e-mail com link → redefinição.
/// O envio real é substituído pelo EmailFake da fábrica.
/// </summary>
[Collection(ApiCollection.Nome)]
public class RecuperacaoSenhaTests(FiooApiFactory api)
{
    private const string NovaSenha = "NovaSenha@123";

    private Task<HttpResponseMessage> Solicitar(string email) =>
        api.CreateClient().PostAsJsonAsync("/api/usuarios/esqueci-senha", new { email });

    private Task<HttpResponseMessage> Redefinir(string? token, string? novaSenha = NovaSenha) =>
        api.CreateClient().PostAsJsonAsync("/api/usuarios/redefinir-senha", new { token, novaSenha });

    private static string ExtrairToken(EmailFake.Mensagem msg)
    {
        var m = Regex.Match(msg.Texto!, @"http://localhost:3000/redefinir-senha\?token=([A-Za-z0-9_\-]+)");
        Assert.True(m.Success, "Link de redefinição não encontrado no e-mail.");
        return Uri.UnescapeDataString(m.Groups[1].Value);
    }

    private async Task<(Usuario usuario, string token)> SolicitarComSucesso()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var resp = await Solicitar(usuario.Email);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var msg = Assert.Single(api.Emails.Para(usuario.Email));
        return (usuario, ExtrairToken(msg));
    }

    [Fact]
    public async Task Solicitacao_valida_envia_email_com_template_do_Fioo()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resp = await Solicitar(usuario.Email);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var msg = Assert.Single(api.Emails.Para(usuario.Email));
        Assert.Contains("Recuperação de senha", msg.Assunto);
        Assert.StartsWith("<!DOCTYPE html>", msg.Html.TrimStart());
        Assert.Contains("Fioo", msg.Html);
        Assert.Contains("Redefinir senha", msg.Html);           // botão
        Assert.Contains("30 minutos", msg.Html);
        Assert.Contains("não solicitou", msg.Html);             // aviso para ignorar
        Assert.Contains(WebUtility.HtmlEncode(usuario.Nome), msg.Html);

        var token = ExtrairToken(msg);
        Assert.Contains(WebUtility.HtmlEncode($"http://localhost:3000/redefinir-senha?token={token}"), msg.Html);

        // No banco fica só o hash, com validade de 30 minutos
        var registro = await api.NoBanco(db => db.PasswordResetTokens.SingleAsync(r => r.UsuarioId == usuario.Id));
        Assert.NotEqual(token, registro.TokenHash);
        Assert.InRange((registro.ExpiraEm - registro.DataCriacao).TotalMinutes, 29.9, 30.1);
    }

    [Fact]
    public async Task Email_inexistente_responde_igual_e_nao_envia()
    {
        var existente = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var respExistente = await Solicitar(existente.Email);
        var respInexistente = await Solicitar("ninguem-aqui@teste.com");

        Assert.Equal(HttpStatusCode.OK, respInexistente.StatusCode);
        Assert.Equal(await respExistente.Content.ReadAsStringAsync(), await respInexistente.Content.ReadAsStringAsync());
        Assert.Empty(api.Emails.Para("ninguem-aqui@teste.com"));
    }

    [Fact]
    public async Task Email_invalido_recebe_resposta_generica_sem_envio()
    {
        var resp = await Solicitar("nao-e-email");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Empty(api.Emails.Para("nao-e-email"));
    }

    [Fact]
    public async Task Rota_usada_pelo_frontend_tambem_responde()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var resp = await api.CreateClient().PostAsJsonAsync("/api/auth/esqueci-senha", new { email = usuario.Email });
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Single(api.Emails.Para(usuario.Email));
    }

    [Fact]
    public async Task Token_redefine_a_senha_uma_unica_vez()
    {
        var (usuario, token) = await SolicitarComSucesso();

        var resp = await Redefinir(token);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var login = await api.CreateClient().PostAsJsonAsync("/api/usuarios/login", new { email = usuario.Email, senha = NovaSenha });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var reuso = await Redefinir(token, "Outra@Senha1");
        Assert.Equal(HttpStatusCode.BadRequest, reuso.StatusCode);
    }

    [Fact]
    public async Task Token_expirado_ou_invalido_e_recusado()
    {
        var (usuario, token) = await SolicitarComSucesso();
        await api.NoBanco(db => db.PasswordResetTokens
            .Where(r => r.UsuarioId == usuario.Id)
            .ExecuteUpdateAsync(r => r.SetProperty(x => x.ExpiraEm, DateTime.UtcNow.AddMinutes(-1))));

        Assert.Equal(HttpStatusCode.BadRequest, (await Redefinir(token)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Redefinir("token-que-nao-existe")).StatusCode);
    }

    [Fact]
    public async Task Senha_fraca_e_recusada_sem_consumir_o_token()
    {
        var (_, token) = await SolicitarComSucesso();

        Assert.Equal(HttpStatusCode.BadRequest, (await Redefinir(token, "fraca")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Redefinir(token)).StatusCode);
    }

    [Fact]
    public async Task Nova_solicitacao_invalida_o_link_anterior()
    {
        var (usuario, primeiro) = await SolicitarComSucesso();
        // Libera o intervalo mínimo entre solicitações
        await api.NoBanco(db => db.PasswordResetTokens
            .Where(r => r.UsuarioId == usuario.Id)
            .ExecuteUpdateAsync(r => r.SetProperty(x => x.DataCriacao, DateTime.UtcNow.AddMinutes(-5))));

        await Solicitar(usuario.Email);
        var mensagens = api.Emails.Para(usuario.Email);
        Assert.Equal(2, mensagens.Count);
        var segundo = ExtrairToken(mensagens[1]);

        Assert.Equal(HttpStatusCode.BadRequest, (await Redefinir(primeiro)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Redefinir(segundo)).StatusCode);
    }

    [Fact]
    public async Task Solicitacoes_em_sequencia_nao_disparam_varios_emails()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);

        await Solicitar(usuario.Email);
        var segunda = await Solicitar(usuario.Email);

        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        Assert.Single(api.Emails.Para(usuario.Email));
    }

    [Fact]
    public async Task Falha_no_envio_nao_expoe_erro_ao_cliente()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        api.Emails.Falha = new EmailEnvioException(EmailErro.Conexao, "servidor fora do ar");
        try
        {
            var resp = await Solicitar(usuario.Email);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.DoesNotContain("servidor fora do ar", await resp.Content.ReadAsStringAsync());
        }
        finally
        {
            api.Emails.Falha = null;
        }
    }
}
