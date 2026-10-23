using System.Net;
using System.Net.Http.Json;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fase 0: vínculo "candidatura aceita" entre costureiro e serviço.
/// </summary>
[Collection(ApiCollection.Nome)]
public class VinculoCandidaturaTests(FiooApiFactory api)
{
    private static string UrlAceitar(Servico s, Candidatura c) => $"/api/servicos/{s.Id}/candidaturas/{c.Id}/aceitar";

    private async Task<(Usuario fornecedor, Servico servico)> CenarioServico(ServicoStatus status = ServicoStatus.EmAndamento)
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var servico = await api.CriarServico(fornecedor, status);
        return (fornecedor, servico);
    }

    private Task<Candidatura?> BuscarCandidatura(int id) =>
        api.NoBanco(db => db.Candidaturas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id));

    [Fact]
    public async Task Dono_aceita_candidato_e_demais_pendentes_sao_recusados()
    {
        var (fornecedor, servico) = await CenarioServico();
        var escolhido = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));
        var outro = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));
        var cancelada = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Cancelada);

        var resposta = await api.ClienteDe(fornecedor).PostAsync(UrlAceitar(servico, escolhido), null);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(CandidaturaStatus.Aceita, (await BuscarCandidatura(escolhido.Id))!.Status);
        Assert.Equal(CandidaturaStatus.Recusada, (await BuscarCandidatura(outro.Id))!.Status);
        Assert.Equal(CandidaturaStatus.Cancelada, (await BuscarCandidatura(cancelada.Id))!.Status);
    }

    [Fact]
    public async Task Servico_expoe_o_costureiro_vinculado()
    {
        var (fornecedor, servico) = await CenarioServico();
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var candidatura = await api.CriarCandidatura(servico, costureiro);
        var cliente = api.ClienteDe(fornecedor);

        var antes = await cliente.GetFromJsonAsync<ServicoResumoDto>($"/api/servicos/{servico.Id}");
        await cliente.PostAsync(UrlAceitar(servico, candidatura), null);
        var depois = await cliente.GetFromJsonAsync<ServicoResumoDto>($"/api/servicos/{servico.Id}");

        Assert.Null(antes!.CostureiroVinculado);
        Assert.Equal(costureiro.Id, depois!.CostureiroVinculado?.Id);
    }

    [Fact]
    public async Task Quem_nao_e_dono_nao_aceita_candidato()
    {
        var (_, servico) = await CenarioServico();
        var candidatura = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));
        var outroFornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resposta = await api.ClienteDe(outroFornecedor).PostAsync(UrlAceitar(servico, candidatura), null);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Equal(CandidaturaStatus.Pendente, (await BuscarCandidatura(candidatura.Id))!.Status);
    }

    [Fact]
    public async Task Nao_aceita_segundo_costureiro()
    {
        var (fornecedor, servico) = await CenarioServico();
        await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Aceita);
        var segunda = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));

        var resposta = await api.ClienteDe(fornecedor).PostAsync(UrlAceitar(servico, segunda), null);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Nao_aceita_candidatura_que_nao_esta_pendente()
    {
        var (fornecedor, servico) = await CenarioServico();
        var recusada = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Recusada);

        var resposta = await api.ClienteDe(fornecedor).PostAsync(UrlAceitar(servico, recusada), null);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Nao_aceita_candidato_em_servico_cancelado()
    {
        var (fornecedor, servico) = await CenarioServico(ServicoStatus.Cancelado);
        var candidatura = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));

        var resposta = await api.ClienteDe(fornecedor).PostAsync(UrlAceitar(servico, candidatura), null);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Candidatura_de_outro_servico_retorna_404()
    {
        var (fornecedor, servico) = await CenarioServico();
        var (_, outroServico) = await CenarioServico();
        var candidaturaDeOutro = await api.CriarCandidatura(outroServico, await api.CriarUsuario(UsuarioTipo.Costureiro));

        var resposta = await api.ClienteDe(fornecedor).PostAsync(UrlAceitar(servico, candidaturaDeOutro), null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Sem_token_retorna_401()
    {
        var (_, servico) = await CenarioServico();

        var resposta = await api.CreateClient().GetAsync($"/api/servicos/{servico.Id}/candidaturas");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Somente_o_dono_lista_os_candidatos()
    {
        var (fornecedor, servico) = await CenarioServico();
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        await api.CriarCandidatura(servico, costureiro);

        var doDono = await api.ClienteDe(fornecedor).GetFromJsonAsync<List<CandidatoDto>>($"/api/servicos/{servico.Id}/candidaturas");
        var deOutro = await api.ClienteDe(costureiro).GetAsync($"/api/servicos/{servico.Id}/candidaturas");

        Assert.Equal(costureiro.Id, Assert.Single(doDono!).Usuario.Id);
        Assert.Equal(HttpStatusCode.Forbidden, deOutro.StatusCode);
    }

    [Fact]
    public async Task Banco_impede_duas_candidaturas_aceitas_no_mesmo_servico()
    {
        var (_, servico) = await CenarioServico();
        await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Aceita);
        var outroCostureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            api.CriarCandidatura(servico, outroCostureiro, CandidaturaStatus.Aceita));
    }

    [Fact]
    public async Task Candidatura_usa_o_usuario_do_token_e_ignora_o_corpo()
    {
        var (fornecedor, servico) = await CenarioServico();
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var resposta = await api.ClienteDe(costureiro).PostAsJsonAsync("/api/candidaturas",
            new { servicoId = servico.Id, usuarioId = fornecedor.Id });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var criada = await api.NoBanco(db => db.Candidaturas.SingleAsync(c => c.ServicoId == servico.Id));
        Assert.Equal(costureiro.Id, criada.UsuarioId);
    }

    [Fact]
    public async Task Nao_aceita_nova_candidatura_em_servico_com_costureiro_vinculado()
    {
        var (_, servico) = await CenarioServico();
        await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Aceita);

        var resposta = await api.ClienteDe(await api.CriarUsuario(UsuarioTipo.Costureiro))
            .PostAsJsonAsync("/api/candidaturas", new { servicoId = servico.Id });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Status_da_candidatura_nao_permite_aceitar_nem_alteracao_por_terceiros()
    {
        var (fornecedor, servico) = await CenarioServico();
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var candidatura = await api.CriarCandidatura(servico, costureiro);
        var url = $"/api/candidaturas/{candidatura.Id}/status";

        var aceitarPorAqui = await api.ClienteDe(fornecedor).PutAsJsonAsync(url, CandidaturaStatus.Aceita);
        var terceiro = await api.ClienteDe(await api.CriarUsuario(UsuarioTipo.Costureiro)).PutAsJsonAsync(url, CandidaturaStatus.Cancelada);
        var costureiroRecusando = await api.ClienteDe(costureiro).PutAsJsonAsync(url, CandidaturaStatus.Recusada);
        var donoRecusando = await api.ClienteDe(fornecedor).PutAsJsonAsync(url, CandidaturaStatus.Recusada);

        Assert.Equal(HttpStatusCode.Conflict, aceitarPorAqui.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, terceiro.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, costureiroRecusando.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, donoRecusando.StatusCode);
        Assert.Equal(CandidaturaStatus.Recusada, (await BuscarCandidatura(candidatura.Id))!.Status);
    }

    [Fact]
    public async Task Candidatura_aceita_nao_pode_ser_excluida()
    {
        var (fornecedor, servico) = await CenarioServico();
        var candidatura = await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Aceita);

        var resposta = await api.ClienteDe(fornecedor).DeleteAsync($"/api/candidaturas/{candidatura.Id}");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.NotNull(await BuscarCandidatura(candidatura.Id));
    }
}
