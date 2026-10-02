using System.Net;
using System.Net.Http.Json;
using Fioo.DTOs;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Tests;

/// <summary>
/// Fase 4: avaliação mútua entre fornecedor e costureiro de um serviço concluído.
/// </summary>
[Collection(ApiCollection.Nome)]
public class AvaliacaoTests(FiooApiFactory api)
{
    private sealed record Cenario(Usuario Fornecedor, Usuario Costureiro, Servico Servico);

    private async Task<Cenario> CriarCenario(ServicoStatus status = ServicoStatus.Concluido)
    {
        var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        var servico = await api.CriarServico(fornecedor, status);
        await api.CriarCandidatura(servico, costureiro, CandidaturaStatus.Aceita);
        return new Cenario(fornecedor, costureiro, servico);
    }

    private static object Corpo(Servico s, int nota = 5, int comunicacao = 4, int qualidade = 3, string? comentario = "Ótimo trabalho") =>
        new { servicoId = s.Id, nota, notaComunicacao = comunicacao, notaQualidade = qualidade, comentario };

    private Task<HttpResponseMessage> Avaliar(Usuario quem, object corpo) =>
        api.ClienteDe(quem).PostAsJsonAsync("/api/avaliacoes", corpo);

    [Fact]
    public async Task Costureiro_avalia_fornecedor()
    {
        var c = await CriarCenario();

        var resposta = await Avaliar(c.Costureiro, Corpo(c.Servico));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var avaliacao = (await resposta.Content.ReadFromJsonAsync<AvaliacaoDto>())!;
        Assert.Equal(c.Fornecedor.Id, avaliacao.AvaliadoId);
        Assert.Equal(c.Costureiro.Id, avaliacao.Avaliador.Id);
        Assert.Equal(UsuarioTipo.Fornecedor, avaliacao.PapelAvaliado);
        Assert.Equal((5, 4, 3), (avaliacao.Nota, avaliacao.NotaComunicacao, avaliacao.NotaQualidade));
    }

    [Fact]
    public async Task Fornecedor_avalia_costureiro()
    {
        var c = await CriarCenario();

        var resposta = await Avaliar(c.Fornecedor, Corpo(c.Servico, comentario: null));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var avaliacao = (await resposta.Content.ReadFromJsonAsync<AvaliacaoDto>())!;
        Assert.Equal(c.Costureiro.Id, avaliacao.AvaliadoId);
        Assert.Equal(UsuarioTipo.Costureiro, avaliacao.PapelAvaliado);
        Assert.Null(avaliacao.Comentario);
    }

    [Fact]
    public async Task Avaliacao_duplicada_retorna_409()
    {
        var c = await CriarCenario();
        await Avaliar(c.Costureiro, Corpo(c.Servico));

        var segunda = await Avaliar(c.Costureiro, Corpo(c.Servico, nota: 1));

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
        Assert.Equal(1, await api.NoBanco(db => db.Avaliacoes.CountAsync(a => a.ServicoId == c.Servico.Id)));
    }

    [Theory]
    [InlineData(ServicoStatus.EmAndamento)]
    [InlineData(ServicoStatus.Cancelado)]
    public async Task Servico_nao_concluido_nao_pode_ser_avaliado(ServicoStatus status)
    {
        var c = await CriarCenario(status);

        var resposta = await Avaliar(c.Costureiro, Corpo(c.Servico));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_que_nao_participa_nao_avalia()
    {
        var c = await CriarCenario();
        var recusado = await api.CriarUsuario(UsuarioTipo.Costureiro);
        await api.CriarCandidatura(c.Servico, recusado, CandidaturaStatus.Recusada);
        var outroFornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);

        Assert.Equal(HttpStatusCode.Forbidden, (await Avaliar(recusado, Corpo(c.Servico))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Avaliar(outroFornecedor, Corpo(c.Servico))).StatusCode);
    }

    [Fact]
    public async Task Avaliado_e_sempre_a_outra_parte_mesmo_se_o_corpo_indicar_outro()
    {
        var c = await CriarCenario();

        var resposta = await Avaliar(c.Costureiro, new
        {
            servicoId = c.Servico.Id, nota = 5, notaComunicacao = 5, notaQualidade = 5,
            avaliadoId = c.Costureiro.Id, avaliadorId = c.Fornecedor.Id
        });

        var avaliacao = (await resposta.Content.ReadFromJsonAsync<AvaliacaoDto>())!;
        Assert.Equal(c.Costureiro.Id, avaliacao.Avaliador.Id);
        Assert.Equal(c.Fornecedor.Id, avaliacao.AvaliadoId);
    }

    [Fact]
    public async Task Comentario_acima_de_300_caracteres_retorna_400()
    {
        var c = await CriarCenario();

        var acima = await Avaliar(c.Costureiro, Corpo(c.Servico, comentario: new string('a', 301)));
        var limite = await Avaliar(c.Costureiro, Corpo(c.Servico, comentario: new string('a', 300)));

        Assert.Equal(HttpStatusCode.BadRequest, acima.StatusCode);
        Assert.Equal(HttpStatusCode.Created, limite.StatusCode);
    }

    [Theory]
    [InlineData(0, 3, 3)]
    [InlineData(6, 3, 3)]
    [InlineData(3, 0, 3)]
    [InlineData(3, 6, 3)]
    [InlineData(3, 3, 0)]
    [InlineData(3, 3, 6)]
    public async Task Notas_fora_de_1_a_5_retornam_400(int nota, int comunicacao, int qualidade)
    {
        var c = await CriarCenario();

        var resposta = await Avaliar(c.Costureiro, Corpo(c.Servico, nota, comunicacao, qualidade));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Sem_token_retorna_401()
    {
        var c = await CriarCenario();

        var resposta = await api.CreateClient().PostAsJsonAsync("/api/avaliacoes", Corpo(c.Servico));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Media_e_total_separados_por_papel()
    {
        // Mesmo usuário avaliado como costureiro em dois serviços e como fornecedor em um
        var pessoa = await api.CriarUsuario(UsuarioTipo.Costureiro);
        for (var i = 0; i < 2; i++)
        {
            var fornecedor = await api.CriarUsuario(UsuarioTipo.Fornecedor);
            var servico = await api.CriarServico(fornecedor, ServicoStatus.Concluido);
            await api.CriarCandidatura(servico, pessoa, CandidaturaStatus.Aceita);
            await Avaliar(fornecedor, Corpo(servico, nota: i == 0 ? 5 : 4));
        }
        var servicoProprio = await api.CriarServico(pessoa, ServicoStatus.Concluido);
        var costureiro = await api.CriarUsuario(UsuarioTipo.Costureiro);
        await api.CriarCandidatura(servicoProprio, costureiro, CandidaturaStatus.Aceita);
        await Avaliar(costureiro, Corpo(servicoProprio, nota: 2));

        var resultado = await api.ClienteDe(costureiro).GetFromJsonAsync<AvaliacoesUsuarioDto>($"/api/avaliacoes/usuario/{pessoa.Id}");
        var soComoFornecedor = await api.ClienteDe(costureiro).GetFromJsonAsync<AvaliacoesUsuarioDto>($"/api/avaliacoes/usuario/{pessoa.Id}?papel={(int)UsuarioTipo.Fornecedor}");

        Assert.Equal(4.5, resultado!.ComoCostureiro.Media);
        Assert.Equal(2, resultado.ComoCostureiro.Total);
        Assert.Equal(2, resultado.ComoFornecedor.Media);
        Assert.Equal(1, resultado.ComoFornecedor.Total);
        Assert.Equal(3, resultado.Itens.Count);
        Assert.Single(soComoFornecedor!.Itens);
    }

    [Fact]
    public async Task Usuario_sem_avaliacoes_tem_media_nula()
    {
        var pessoa = await api.CriarUsuario(UsuarioTipo.Costureiro);

        var resultado = await api.ClienteDe(pessoa).GetFromJsonAsync<AvaliacoesUsuarioDto>($"/api/avaliacoes/usuario/{pessoa.Id}");

        Assert.Null(resultado!.ComoCostureiro.Media);
        Assert.Equal(0, resultado.ComoCostureiro.Total);
    }

    [Fact]
    public async Task Lista_avaliacoes_do_servico()
    {
        var c = await CriarCenario();
        await Avaliar(c.Costureiro, Corpo(c.Servico));
        await Avaliar(c.Fornecedor, Corpo(c.Servico));

        var lista = await api.ClienteDe(c.Costureiro).GetFromJsonAsync<List<AvaliacaoDto>>($"/api/avaliacoes/servico/{c.Servico.Id}");

        Assert.Equal(2, lista!.Count);
    }

    [Fact]
    public async Task Servico_avaliado_nao_pode_ser_excluido()
    {
        var c = await CriarCenario();
        await Avaliar(c.Costureiro, Corpo(c.Servico));

        var resposta = await api.ClienteDe(c.Fornecedor).DeleteAsync($"/api/servicos/{c.Servico.Id}");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    // ── Garantias do próprio banco, independentes da API ──

    private Task InserirDireto(Cenario c, Action<Avaliacao> ajustar) => api.NoBanco(async db =>
    {
        var avaliacao = new Avaliacao
        {
            ServicoId = c.Servico.Id,
            AvaliadorId = c.Costureiro.Id,
            AvaliadoId = c.Fornecedor.Id,
            PapelAvaliado = UsuarioTipo.Fornecedor,
            Nota = 5, NotaComunicacao = 5, NotaQualidade = 5,
            DataAvaliacao = DateTime.UtcNow
        };
        ajustar(avaliacao);
        db.Avaliacoes.Add(avaliacao);
        return await db.SaveChangesAsync();
    });

    [Fact]
    public async Task Banco_recusa_nota_fora_de_1_a_5()
    {
        var c = await CriarCenario();

        await Assert.ThrowsAsync<DbUpdateException>(() => InserirDireto(c, a => a.NotaQualidade = 6));
        await Assert.ThrowsAsync<DbUpdateException>(() => InserirDireto(c, a => a.Nota = 0));
    }

    [Fact]
    public async Task Banco_recusa_auto_avaliacao()
    {
        var c = await CriarCenario();

        await Assert.ThrowsAsync<DbUpdateException>(() => InserirDireto(c, a => a.AvaliadoId = a.AvaliadorId));
    }

    [Fact]
    public async Task Banco_recusa_avaliacao_duplicada()
    {
        var c = await CriarCenario();
        await InserirDireto(c, _ => { });

        await Assert.ThrowsAsync<DbUpdateException>(() => InserirDireto(c, _ => { }));
    }

    [Fact]
    public async Task Banco_recusa_comentario_acima_de_300()
    {
        var c = await CriarCenario();

        await Assert.ThrowsAsync<DbUpdateException>(() => InserirDireto(c, a => a.Comentario = new string('x', 301)));
    }
}
