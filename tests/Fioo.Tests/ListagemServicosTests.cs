using System.Net;
using System.Net.Http.Json;
using Fioo.Controller.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;

namespace Fioo.Tests;

/// <summary>
/// Fase 3: filtros, ordenação e paginação de GET /api/servicos.
/// Cada teste usa um marcador único no título e filtra por ele (busca),
/// para não enxergar os serviços criados pelos outros testes.
/// </summary>
[Collection(ApiCollection.Nome)]
public class ListagemServicosTests(FiooApiFactory api)
{
    private static string Marcador() => "m" + Guid.NewGuid().ToString("N")[..10];

    private async Task<Servico> Servico(Usuario dono, string marcador, Action<Servico>? configurar = null,
        ServicoStatus status = ServicoStatus.EmAndamento) =>
        await api.CriarServico(dono, status, s => { s.Titulo = $"Serviço {marcador}"; configurar?.Invoke(s); });

    private async Task<PaginaServicosDto> Listar(string query)
    {
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var resposta = await api.ClienteDe(costureiro).GetAsync($"/api/servicos?{query}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<PaginaServicosDto>())!;
    }

    private async Task<List<int>> Ids(string query) => (await Listar(query)).Itens.Select(i => i.Id).ToList();

    [Fact]
    public async Task Busca_ignora_maiusculas_e_acentos()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var calca = await Servico(dono, m, s => s.Titulo = $"Calça Jeans {m}");
        await Servico(dono, m, s => s.Titulo = $"Camiseta {m}");

        Assert.Equal([calca.Id], await Ids($"busca=CALCA jeans {m}"));
    }

    [Fact]
    public async Task Filtros_combinados()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        void Base(Servico s)
        {
            s.Estado = "SC"; s.Cidade = "Blumenau"; s.Valor = 10m; s.TipoCobranca = CobrancaTipo.PorOperacao;
            s.TipoPrazo = PrazoTipo.Quinzenal; s.CategoriaServico = "Bordado";
        }
        var alvo = await Servico(dono, m, Base);
        await Servico(dono, m, s => { Base(s); s.Estado = "PR"; });
        await Servico(dono, m, s => { Base(s); s.Cidade = "Gaspar"; });
        await Servico(dono, m, s => { Base(s); s.Valor = 50m; });
        await Servico(dono, m, s => { Base(s); s.Valor = null; });
        await Servico(dono, m, s => { Base(s); s.TipoCobranca = CobrancaTipo.PorPeca; });
        await Servico(dono, m, s => { Base(s); s.TipoPrazo = PrazoTipo.Mensal; });
        await Servico(dono, m, s => { Base(s); s.CategoriaServico = "Costura"; });
        await Servico(dono, m, Base, ServicoStatus.Cancelado);

        var ids = await Ids($"busca={m}&uf=sc&cidade=blumenáu&valorMin=5&valorMax=20&cobranca=1&prazo=1&categoria=BORDÁDO&status=1");

        Assert.Equal([alvo.Id], ids);
    }

    [Fact]
    public async Task Servico_sem_localizacao_nao_entra_no_filtro_de_localizacao()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        await Servico(dono, m);

        Assert.Empty(await Ids($"busca={m}&uf=SC"));
        Assert.Empty(await Ids($"busca={m}&cidade=Blumenau"));
        Assert.Single(await Ids($"busca={m}"));
    }

    [Fact]
    public async Task Nao_lista_os_servicos_do_proprio_usuario()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        await Servico(dono, m);

        var resposta = await api.ClienteDe(dono).GetFromJsonAsync<PaginaServicosDto>($"/api/servicos?busca={m}");

        Assert.Empty(resposta!.Itens);
    }

    [Fact]
    public async Task Mais_relevantes_poe_em_andamento_primeiro_e_depois_mais_recentes()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var agora = DateTime.UtcNow;
        var antigo = await Servico(dono, m, s => s.DataCriacao = agora.AddDays(-10));
        var concluidoRecente = await Servico(dono, m, s => s.DataCriacao = agora, ServicoStatus.Concluido);
        var recente = await Servico(dono, m, s => s.DataCriacao = agora.AddDays(-1));

        Assert.Equal([recente.Id, antigo.Id, concluidoRecente.Id], await Ids($"busca={m}"));
        Assert.Equal([recente.Id, antigo.Id, concluidoRecente.Id], await Ids($"busca={m}&ordenacao=relevantes"));
    }

    [Fact]
    public async Task Ordena_por_prazo_com_sem_prazo_por_ultimo()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var semPrazo = await Servico(dono, m, s => { s.TipoPrazo = null; s.DataReferenciaPrazo = null; });
        var longe = await Servico(dono, m, s => s.DataReferenciaPrazo = new DateOnly(2031, 1, 1));
        var perto = await Servico(dono, m, s => s.DataReferenciaPrazo = new DateOnly(2030, 1, 1));

        Assert.Equal([perto.Id, longe.Id, semPrazo.Id], await Ids($"busca={m}&ordenacao=prazo-proximo"));
        Assert.Equal([longe.Id, perto.Id, semPrazo.Id], await Ids($"busca={m}&ordenacao=prazo-distante"));
    }

    [Fact]
    public async Task Ordena_por_valor_com_a_combinar_por_ultimo()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var aCombinar = await Servico(dono, m, s => s.Valor = null);
        var caro = await Servico(dono, m, s => s.Valor = 100m);
        var barato = await Servico(dono, m, s => s.Valor = 1m);

        Assert.Equal([caro.Id, barato.Id, aCombinar.Id], await Ids($"busca={m}&ordenacao=maior-valor"));
        Assert.Equal([barato.Id, caro.Id, aCombinar.Id], await Ids($"busca={m}&ordenacao=menor-valor"));
    }

    [Fact]
    public async Task Paginacao_estavel_com_empates()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var mesmaData = DateTime.UtcNow;
        var criados = new List<int>();
        for (var i = 0; i < 5; i++)
            criados.Add((await Servico(dono, m, s => { s.DataCriacao = mesmaData; s.Valor = 10m; })).Id);

        var vistos = new List<int>();
        PaginaServicosDto pagina;
        var numero = 1;
        do
        {
            pagina = await Listar($"busca={m}&ordenacao=maior-valor&tamanhoPagina=2&pagina={numero++}");
            vistos.AddRange(pagina.Itens.Select(i => i.Id));
        } while (pagina.TemMais);

        Assert.Equal(criados.OrderDescending(), vistos);
    }

    [Theory]
    [InlineData("valorMin=20&valorMax=10")]
    [InlineData("valorMin=-1")]
    [InlineData("ordenacao=aleatoria")]
    [InlineData("status=0")]
    [InlineData("cobranca=9")]
    [InlineData("prazo=9")]
    [InlineData("uf=SCX")]
    [InlineData("pagina=0")]
    [InlineData("tamanhoPagina=500")]
    public async Task Filtro_invalido_retorna_400(string query)
    {
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var resposta = await api.ClienteDe(costureiro).GetAsync($"/api/servicos?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Categorias_sem_repetir_variacoes_de_maiusculas_e_acentos()
    {
        var m = Marcador();
        var dono = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        await Servico(dono, m, s => s.CategoriaServico = $"Acabamento {m}");
        await Servico(dono, m, s => s.CategoriaServico = $"acabaménto {m.ToUpperInvariant()} ");
        await Servico(dono, m, s => s.CategoriaServico = $"Overloque {m}");

        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var categorias = await api.ClienteDe(costureiro).GetFromJsonAsync<List<string>>("/api/servicos/categorias");

        Assert.Single(categorias!, c => c.Contains(m, StringComparison.OrdinalIgnoreCase) && c.StartsWith("acaba", StringComparison.OrdinalIgnoreCase));
        Assert.Single(categorias!, c => c == $"Overloque {m}");
    }
}
