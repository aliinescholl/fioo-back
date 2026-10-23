using System.Net;
using System.Net.Http.Json;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Fioo.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fase 2: status do serviço (Em andamento → Concluído | Cancelado).
/// </summary>
[Collection(ApiCollection.Nome)]
public class ServicoStatusTests(FiooApiFactory api)
{
    private static string UrlStatus(Servico s) => $"/api/servicos/{s.Id}/status";

    private Task<ServicoStatus> StatusNoBanco(Servico s) =>
        api.NoBanco(db => db.Servicos.Where(x => x.Id == s.Id).Select(x => x.Status).SingleAsync());

    private async Task<(Usuario fornecedor, Servico servico)> Cenario(ServicoStatus status = ServicoStatus.EmAndamento, bool comCostureiro = false)
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var servico = await api.CriarServico(fornecedor, status);
        if (comCostureiro)
            await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro), CandidaturaStatus.Aceita);
        return (fornecedor, servico);
    }

    [Theory]
    [InlineData(ServicoStatus.EmAndamento, ServicoStatus.Concluido, true)]
    [InlineData(ServicoStatus.EmAndamento, ServicoStatus.Cancelado, true)]
    [InlineData(ServicoStatus.EmAndamento, ServicoStatus.EmAndamento, false)]
    [InlineData(ServicoStatus.Concluido, ServicoStatus.Cancelado, false)]
    [InlineData(ServicoStatus.Concluido, ServicoStatus.EmAndamento, false)]
    [InlineData(ServicoStatus.Cancelado, ServicoStatus.EmAndamento, false)]
    [InlineData(ServicoStatus.Cancelado, ServicoStatus.Concluido, false)]
    public void Regras_de_transicao(ServicoStatus atual, ServicoStatus novo, bool permitida)
    {
        Assert.Equal(permitida, ServicoStatusRegras.TransicaoPermitida(atual, novo));
    }

    [Fact]
    public async Task Servico_novo_nasce_em_andamento_mesmo_enviando_outro_status()
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resposta = await api.ClienteDe(fornecedor).PostAsJsonAsync("/api/servicos", new
        {
            titulo = "Bordado",
            tipoCobranca = 0,
            tipoPrazo = (int)PrazoTipo.Semanal,
            status = (int)ServicoStatus.Cancelado
        });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criado = await resposta.Content.ReadFromJsonAsync<Servico>();
        Assert.Equal(ServicoStatus.EmAndamento, await StatusNoBanco(criado!));
    }

    [Fact]
    public async Task Edicao_do_formulario_nao_altera_status()
    {
        var (fornecedor, servico) = await Cenario();

        await api.ClienteDe(fornecedor).PutAsJsonAsync($"/api/servicos/{servico.Id}", new
        {
            titulo = "Novo título",
            tipoCobranca = 0,
            tipoPrazo = (int)PrazoTipo.Mensal,
            status = (int)ServicoStatus.Cancelado
        });

        Assert.Equal(ServicoStatus.EmAndamento, await StatusNoBanco(servico));
    }

    [Fact]
    public async Task Dono_cancela_servico_em_andamento()
    {
        var (fornecedor, servico) = await Cenario();

        var resposta = await api.ClienteDe(fornecedor).PatchAsJsonAsync(UrlStatus(servico), new { status = ServicoStatus.Cancelado });

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(ServicoStatus.Cancelado, await StatusNoBanco(servico));
    }

    [Fact]
    public async Task Dono_conclui_servico_com_costureiro_vinculado()
    {
        var (fornecedor, servico) = await Cenario(comCostureiro: true);

        var resposta = await api.ClienteDe(fornecedor).PatchAsJsonAsync(UrlStatus(servico), new { status = ServicoStatus.Concluido });

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(ServicoStatus.Concluido, await StatusNoBanco(servico));
    }

    [Fact]
    public async Task Concluir_sem_costureiro_vinculado_retorna_422()
    {
        var (fornecedor, servico) = await Cenario();
        // Candidatura pendente não é vínculo
        await api.CriarCandidatura(servico, await api.CriarUsuario(UsuarioTipo.Costureiro));

        var resposta = await api.ClienteDe(fornecedor).PatchAsJsonAsync(UrlStatus(servico), new { status = ServicoStatus.Concluido });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(ServicoStatus.EmAndamento, await StatusNoBanco(servico));
    }

    [Theory]
    [InlineData(ServicoStatus.Concluido, ServicoStatus.Cancelado)]
    [InlineData(ServicoStatus.Cancelado, ServicoStatus.EmAndamento)]
    [InlineData(ServicoStatus.EmAndamento, ServicoStatus.EmAndamento)]
    public async Task Transicao_invalida_retorna_409(ServicoStatus atual, ServicoStatus novo)
    {
        var (fornecedor, servico) = await Cenario(atual, comCostureiro: true);

        var resposta = await api.ClienteDe(fornecedor).PatchAsJsonAsync(UrlStatus(servico), new { status = novo });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal(atual, await StatusNoBanco(servico));
    }

    [Fact]
    public async Task Quem_nao_e_dono_nao_altera_status()
    {
        var (_, servico) = await Cenario();
        var outro = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        var resposta = await api.ClienteDe(outro).PatchAsJsonAsync(UrlStatus(servico), new { status = ServicoStatus.Cancelado });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Equal(ServicoStatus.EmAndamento, await StatusNoBanco(servico));
    }

    [Theory]
    [InlineData(0)] // antigo "Ativo"
    [InlineData(7)]
    public async Task Status_inexistente_retorna_400(int status)
    {
        var (fornecedor, servico) = await Cenario();

        var resposta = await api.ClienteDe(fornecedor).PatchAsJsonAsync(UrlStatus(servico), new { status });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Theory]
    [InlineData(ServicoStatus.Concluido)]
    [InlineData(ServicoStatus.Cancelado)]
    public async Task Candidatura_so_em_servico_em_andamento(ServicoStatus status)
    {
        var (_, servico) = await Cenario(status);
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var resposta = await api.ClienteDe(costureiro).PostAsJsonAsync("/api/candidaturas", new { servicoId = servico.Id });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Banco_recusa_status_fora_da_lista()
    {
        var (_, servico) = await Cenario();

        await Assert.ThrowsAnyAsync<Exception>(() => api.NoBanco(db =>
            db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Servicos\" SET \"Status\" = 'Ativo' WHERE \"Id\" = {servico.Id}")));
    }
}
