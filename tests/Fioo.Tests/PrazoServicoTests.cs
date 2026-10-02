using System.Net;
using System.Net.Http.Json;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Fioo.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fase 1: prazo de entrega no cadastro/edição de serviço.
/// </summary>
[Collection(ApiCollection.Nome)]
public class PrazoServicoTests(FiooApiFactory api)
{
    private static object Payload(PrazoTipo? tipoPrazo, string? dataPrazo) => new
    {
        titulo = "Costura de camisetas",
        tipoCobranca = (int)CobrancaTipo.PorPeca,
        valor = 1.5m,
        tipoPrazo = (int?)tipoPrazo,
        dataPrazo,
        status = (int)ServicoStatus.Ativo
    };

    private async Task<HttpResponseMessage> Cadastrar(object payload)
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        return await api.ClienteDe(fornecedor).PostAsJsonAsync("/api/servicos", payload);
    }

    private async Task<Servico> ServicoCriado(HttpResponseMessage resposta)
    {
        var criado = await resposta.Content.ReadFromJsonAsync<Servico>();
        return await api.NoBanco(db => db.Servicos.AsNoTracking().SingleAsync(s => s.Id == criado!.Id));
    }

    [Theory]
    [InlineData(PrazoTipo.Semanal, 7)]
    [InlineData(PrazoTipo.Quinzenal, 15)]
    [InlineData(PrazoTipo.Mensal, 30)]
    public async Task Prazo_relativo_sem_data_e_aceito(PrazoTipo tipo, int dias)
    {
        var resposta = await Cadastrar(Payload(tipo, null));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var servico = await ServicoCriado(resposta);
        Assert.Null(servico.DataPrazo);
        var criacaoEmBrasilia = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(servico.DataCriacao, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")));
        Assert.Equal(criacaoEmBrasilia.AddDays(dias), servico.DataReferenciaPrazo);
    }

    [Fact]
    public async Task Prazo_relativo_ignora_data_enviada()
    {
        var resposta = await Cadastrar(Payload(PrazoTipo.Quinzenal, "2030-01-01"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Null((await ServicoCriado(resposta)).DataPrazo);
    }

    [Fact]
    public async Task Data_especifica_com_data_e_aceita()
    {
        var resposta = await Cadastrar(Payload(PrazoTipo.DataEspecifica, "2030-05-20"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var servico = await ServicoCriado(resposta);
        Assert.Equal(new DateOnly(2030, 5, 20), servico.DataPrazo);
        Assert.Equal(new DateOnly(2030, 5, 20), servico.DataReferenciaPrazo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("20/05/2030")]
    public async Task Data_especifica_sem_data_valida_e_recusada(string? data)
    {
        var resposta = await Cadastrar(Payload(PrazoTipo.DataEspecifica, data));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Tipo_de_prazo_e_obrigatorio()
    {
        var resposta = await Cadastrar(Payload(null, null));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("prazo de entrega", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Edicao_de_data_especifica_para_mensal_limpa_a_data()
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var cliente = api.ClienteDe(fornecedor);
        var criado = await ServicoCriado(await cliente.PostAsJsonAsync("/api/servicos", Payload(PrazoTipo.DataEspecifica, "2030-05-20")));

        var resposta = await cliente.PutAsJsonAsync($"/api/servicos/{criado.Id}", Payload(PrazoTipo.Mensal, "2030-05-20"));

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var editado = await api.NoBanco(db => db.Servicos.AsNoTracking().SingleAsync(s => s.Id == criado.Id));
        Assert.Null(editado.DataPrazo);
        Assert.Equal(PrazoHelper.CalcularDataReferencia(PrazoTipo.Mensal, null, editado.DataCriacao), editado.DataReferenciaPrazo);
    }

    [Fact]
    public async Task Edicao_por_quem_nao_e_dono_retorna_403()
    {
        var (dono, outro) = (await api.CriarUsuario(UsuarioTipo.Fornecedor), await api.CriarUsuario(UsuarioTipo.Fornecedor));
        var servico = await api.CriarServico(dono, ServicoStatus.Ativo);

        var resposta = await api.ClienteDe(outro).PutAsJsonAsync($"/api/servicos/{servico.Id}", Payload(PrazoTipo.Semanal, null));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public void Data_de_referencia_usa_o_fuso_de_brasilia()
    {
        // 10/01 01:00 UTC = 09/01 22:00 em Brasília
        var criacao = new DateTime(2026, 1, 10, 1, 0, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 1, 16), PrazoHelper.CalcularDataReferencia(PrazoTipo.Semanal, null, criacao));
        Assert.Equal(new DateOnly(2026, 1, 24), PrazoHelper.CalcularDataReferencia(PrazoTipo.Quinzenal, null, criacao));
        Assert.Equal(new DateOnly(2026, 2, 8), PrazoHelper.CalcularDataReferencia(PrazoTipo.Mensal, null, criacao));
        Assert.Null(PrazoHelper.CalcularDataReferencia(null, null, criacao));
    }
}
