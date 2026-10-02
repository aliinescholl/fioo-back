using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Fioo.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Pendências do relatório final: senha com salt, dados privados, exclusão de conta,
/// mensagens de erro em pt-BR e categorias sem duplicidade.
/// </summary>
[Collection(ApiCollection.Nome)]
public class PendenciasTests(FiooApiFactory api)
{
    private const string Senha = "Senha@123";

    private Task<string> HashNoBanco(int id) =>
        api.NoBanco(db => db.Usuarios.Where(u => u.Id == id).Select(u => u.SenhaHash).SingleAsync());

    private Task DefinirHash(int id, string hash) => api.NoBanco(async db =>
    {
        var u = await db.Usuarios.SingleAsync(x => x.Id == id);
        u.SenhaHash = hash;
        return await db.SaveChangesAsync();
    });

    private Task<HttpResponseMessage> Login(Usuario u, string senha = Senha) =>
        api.CreateClient().PostAsJsonAsync("/api/usuarios/login", new { email = u.Email, senha });

    // ── Senha ──

    [Fact]
    public void Hash_novo_tem_salt_e_e_verificado()
    {
        var a = SenhaHasher.Gerar(Senha);
        var b = SenhaHasher.Gerar(Senha);

        Assert.NotEqual(a, b);
        Assert.StartsWith("pbkdf2$", a);
        Assert.True(SenhaHasher.Verificar(Senha, a));
        Assert.False(SenhaHasher.Verificar("outra", a));
        Assert.False(SenhaHasher.PrecisaAtualizar(a));
    }

    [Fact]
    public async Task Login_com_hash_antigo_funciona_e_atualiza_para_pbkdf2()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var legado = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(Senha)));
        await DefinirHash(usuario.Id, legado);

        var primeiro = await Login(usuario);
        var hashDepois = await HashNoBanco(usuario.Id);
        var segundo = await Login(usuario);
        var errada = await Login(usuario, "Errada@123");

        Assert.Equal(HttpStatusCode.OK, primeiro.StatusCode);
        Assert.StartsWith("pbkdf2$", hashDepois);
        Assert.Equal(HttpStatusCode.OK, segundo.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, errada.StatusCode);
    }

    [Fact]
    public async Task Cadastro_grava_hash_novo_e_nao_devolve_o_hash()
    {
        var email = $"novo{Guid.NewGuid():N}@teste.com";

        var resposta = await api.CreateClient().PostAsJsonAsync("/api/usuarios",
            new { nome = "Nova Pessoa", email, senha = Senha, ehCostureiro = true });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.DoesNotContain("senhaHash", await resposta.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        var hash = await api.NoBanco(db => db.Usuarios.Where(u => u.Email == email).Select(u => u.SenhaHash).SingleAsync());
        Assert.StartsWith("pbkdf2$", hash);
    }

    // ── Dados privados ──

    [Fact]
    public async Task Me_nao_devolve_o_hash()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var json = await api.ClienteDe(usuario).GetStringAsync("/api/usuarios/me");

        Assert.Contains(usuario.Email, json);
        Assert.DoesNotContain("senhaHash", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Usuario_por_id_completo_so_para_o_proprio()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var outro = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var proprio = await api.ClienteDe(usuario).GetStringAsync($"/api/usuarios/{usuario.Id}");
        var deOutro = await api.ClienteDe(outro).GetStringAsync($"/api/usuarios/{usuario.Id}");
        var semToken = await api.CreateClient().GetAsync($"/api/usuarios/{usuario.Id}");

        Assert.Contains(usuario.Email, proprio);
        Assert.DoesNotContain(usuario.Email, deOutro);
        Assert.Contains(usuario.NomeUsuario, deOutro);
        Assert.Equal(HttpStatusCode.Unauthorized, semToken.StatusCode);
    }

    [Fact]
    public async Task Candidaturas_em_andamento_so_do_proprio_usuario()
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var outro = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var proprio = await api.ClienteDe(usuario).GetAsync($"/api/candidaturas/em-andamento/{usuario.Id}");
        var deOutro = await api.ClienteDe(outro).GetAsync($"/api/candidaturas/em-andamento/{usuario.Id}");

        Assert.Equal(HttpStatusCode.OK, proprio.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deOutro.StatusCode);
    }

    // ── Exclusão de conta ──

    [Fact]
    public async Task Exclusao_de_conta()
    {
        var semHistorico = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var comCandidatura = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        await api.CriarCandidatura(await api.CriarServico(fornecedor, ServicoStatus.EmAndamento), comCandidatura);

        var deOutro = await api.ClienteDe(fornecedor).DeleteAsync($"/api/usuarios/{semHistorico.Id}");
        var semToken = await api.CreateClient().DeleteAsync($"/api/usuarios/{semHistorico.Id}");
        var bloqueada = await api.ClienteDe(comCandidatura).DeleteAsync($"/api/usuarios/{comCandidatura.Id}");
        var propria = await api.ClienteDe(semHistorico).DeleteAsync($"/api/usuarios/{semHistorico.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, deOutro.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, semToken.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, bloqueada.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, propria.StatusCode);
        Assert.False(await api.NoBanco(db => db.Usuarios.AnyAsync(u => u.Id == semHistorico.Id)));
    }

    // ── Erros em pt-BR ──

    [Theory]
    [InlineData("/api/servicos?valorMin=abc", "valorMin")]
    [InlineData("/api/servicos?pagina=x", "pagina")]
    public async Task Parametro_em_formato_invalido_retorna_mensagem_em_portugues(string url, string campo)
    {
        var usuario = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var resposta = await api.ClienteDe(usuario).GetAsync(url);
        var json = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains($"\"field\":\"{campo}\"", json);
        Assert.Contains("valor inválido", json);
    }

    [Fact]
    public async Task Json_malformado_retorna_mensagem_em_portugues()
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resposta = await api.ClienteDe(fornecedor).PostAsync("/api/servicos",
            new StringContent("{ \"titulo\": ", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("inválido", await resposta.Content.ReadAsStringAsync());
    }

    // ── Serviço: categorias e limites ──

    private static object Servico(string titulo, string? categoria) =>
        new { titulo, tipoCobranca = 0, tipoPrazo = (int)PrazoTipo.Semanal, categoriaServico = categoria };

    [Fact]
    public async Task Categoria_com_outra_grafia_usa_a_ja_cadastrada()
    {
        var m = Guid.NewGuid().ToString("N")[..8];
        var cliente = api.ClienteDe(await api.CriarUsuario(UsuarioTipo.Fornecedor));
        await cliente.PostAsJsonAsync("/api/servicos", Servico("A", $"Bordado {m}"));

        var resposta = await cliente.PostAsJsonAsync("/api/servicos", Servico("B", $"  bordádo {m.ToUpperInvariant()} "));
        var criado = await resposta.Content.ReadFromJsonAsync<Servico>();

        Assert.Equal($"Bordado {m}", criado!.CategoriaServico);
    }

    [Theory]
    [InlineData(201, null, null)]
    [InlineData(10, "SCX", null)]
    [InlineData(10, null, 101)]
    public async Task Texto_acima_do_limite_retorna_400(int tamanhoTitulo, string? uf, int? tamanhoCategoria)
    {
        var cliente = api.ClienteDe(await api.CriarUsuario(UsuarioTipo.Fornecedor));

        var resposta = await cliente.PostAsJsonAsync("/api/servicos", new
        {
            titulo = new string('t', tamanhoTitulo),
            tipoCobranca = 0,
            tipoPrazo = (int)PrazoTipo.Semanal,
            estado = uf,
            categoriaServico = tamanhoCategoria is null ? null : new string('c', tamanhoCategoria.Value)
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
