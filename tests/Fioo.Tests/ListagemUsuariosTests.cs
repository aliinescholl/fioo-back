using System.Net;
using System.Net.Http.Json;
using Fioo.Controller.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fase 6: filtros e ordenação da tela Encontrar (GET /api/usuarios/costureiros|fornecedores).
/// Cada teste usa um marcador único no nome e busca por ele.
/// </summary>
[Collection(ApiCollection.Nome)]
public class ListagemUsuariosTests(FiooApiFactory api)
{
    private static string Marcador() => "m" + Guid.NewGuid().ToString("N")[..10];

    private async Task<Usuario> Costureiro(string marcador, string nome = "Pessoa", string? uf = null, string? cidade = null)
    {
        var criado = await api.CriarUsuario(UsuarioTipo.Costureiro);
        return await api.NoBanco(async db =>
        {
            var u = await db.Usuarios.SingleAsync(x => x.Id == criado.Id);
            u.Nome = $"{nome} {marcador}";
            u.Estado = uf;
            u.Cidade = cidade;
            await db.SaveChangesAsync();
            return u;
        });
    }

    // Registra avaliações recebidas por "avaliado" no papel indicado, cada uma num serviço concluído
    private async Task Avaliacoes(Usuario avaliado, UsuarioTipo papel, params short[] notas)
    {
        foreach (var nota in notas)
        {
            var outro = await api.CriarUsuario(papel == UsuarioTipo.Costureiro ? UsuarioTipo.Fornecedor : UsuarioTipo.Costureiro);
            var dono = papel == UsuarioTipo.Fornecedor ? avaliado : outro;
            var servico = await api.CriarServico(dono, ServicoStatus.Concluido);
            await api.NoBanco(async db =>
            {
                db.Avaliacoes.Add(new Avaliacao
                {
                    ServicoId = servico.Id, AvaliadorId = outro.Id, AvaliadoId = avaliado.Id, PapelAvaliado = papel,
                    Nota = nota, NotaComunicacao = nota, NotaQualidade = nota, DataAvaliacao = DateTime.UtcNow
                });
                return await db.SaveChangesAsync();
            });
        }
    }

    private async Task<PaginaUsuariosDto> Listar(string query, string tipo = "costureiros")
    {
        var quem = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var resposta = await api.ClienteDe(quem).GetAsync($"/api/usuarios/{tipo}?{query}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<PaginaUsuariosDto>())!;
    }

    private async Task<List<int>> Ids(string query) => (await Listar(query)).Itens.Select(i => i.Id).ToList();

    [Fact]
    public async Task Busca_por_nome_ignora_maiusculas_e_acentos()
    {
        var m = Marcador();
        var joao = await Costureiro(m, "João Conceição");
        await Costureiro(m, "Maria");

        Assert.Equal([joao.Id], await Ids($"busca=JOAO conceicao {m}"));
    }

    [Fact]
    public async Task Filtra_por_uf_e_cidade()
    {
        var m = Marcador();
        var alvo = await Costureiro(m, uf: "SC", cidade: "São José");
        await Costureiro(m, uf: "SC", cidade: "Blumenau");
        await Costureiro(m, uf: "PR", cidade: "São José dos Pinhais");
        await Costureiro(m);

        Assert.Equal([alvo.Id], await Ids($"busca={m}&uf=sc&cidade=sao jose"));
    }

    [Fact]
    public async Task Mais_relevantes_maior_media_depois_mais_avaliacoes_e_sem_avaliacao_por_ultimo()
    {
        var m = Marcador();
        var semNota = await Costureiro(m, "A");
        var media4Com1 = await Costureiro(m, "B");
        var media4Com3 = await Costureiro(m, "C");
        var media5 = await Costureiro(m, "D");
        await Avaliacoes(media4Com1, UsuarioTipo.Costureiro, 4);
        await Avaliacoes(media4Com3, UsuarioTipo.Costureiro, 4, 4, 4);
        await Avaliacoes(media5, UsuarioTipo.Costureiro, 5);

        Assert.Equal([media5.Id, media4Com3.Id, media4Com1.Id, semNota.Id], await Ids($"busca={m}"));
        Assert.Equal([media5.Id, media4Com3.Id, media4Com1.Id, semNota.Id], await Ids($"busca={m}&ordenacao=avaliacao-alta"));
        Assert.Equal([media4Com3.Id, media4Com1.Id, media5.Id, semNota.Id], await Ids($"busca={m}&ordenacao=avaliacao-baixa"));
    }

    [Fact]
    public async Task Avaliacao_minima_exclui_quem_nao_tem_ou_tem_menos()
    {
        var m = Marcador();
        await Costureiro(m, "Sem");
        var tres = await Costureiro(m, "Tres");
        var cinco = await Costureiro(m, "Cinco");
        await Avaliacoes(tres, UsuarioTipo.Costureiro, 3);
        await Avaliacoes(cinco, UsuarioTipo.Costureiro, 5);

        Assert.Equal([cinco.Id], await Ids($"busca={m}&avaliacaoMin=4"));
        Assert.Equal([cinco.Id, tres.Id], await Ids($"busca={m}&avaliacaoMin=3"));
    }

    [Fact]
    public async Task Media_considera_so_o_papel_listado()
    {
        var m = Marcador();
        var pessoa = await Costureiro(m);
        await Avaliacoes(pessoa, UsuarioTipo.Costureiro, 5);
        await Avaliacoes(pessoa, UsuarioTipo.Fornecedor, 1, 1);

        var item = Assert.Single((await Listar($"busca={m}")).Itens);

        Assert.Equal(5, item.Media);
        Assert.Equal(1, item.TotalAvaliacoes);
    }

    [Fact]
    public async Task Ordem_alfabetica_ignora_acentos_e_maiusculas()
    {
        var m = Marcador();
        var bruno = await Costureiro(m, "Bruno");
        var alvaro = await Costureiro(m, "Álvaro");
        var alice = await Costureiro(m, "alice");

        Assert.Equal([alice.Id, alvaro.Id, bruno.Id], await Ids($"busca={m}&ordenacao=az"));
        Assert.Equal([bruno.Id, alvaro.Id, alice.Id], await Ids($"busca={m}&ordenacao=za"));
    }

    [Fact]
    public async Task Lista_so_o_tipo_pedido()
    {
        var m = Marcador();
        await Costureiro(m);

        Assert.Empty((await Listar($"busca={m}", "fornecedores")).Itens);
    }

    [Theory]
    [InlineData("avaliacaoMin=0")]
    [InlineData("avaliacaoMin=6")]
    [InlineData("ordenacao=aleatoria")]
    [InlineData("uf=XYZ")]
    [InlineData("tamanhoPagina=0")]
    public async Task Parametro_invalido_retorna_400(string query)
    {
        var quem = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resposta = await api.ClienteDe(quem).GetAsync($"/api/usuarios/costureiros?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Sem_token_retorna_401()
    {
        var resposta = await api.CreateClient().GetAsync("/api/usuarios/fornecedores");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
