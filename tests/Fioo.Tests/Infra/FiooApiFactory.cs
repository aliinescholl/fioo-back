using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Fioo.Data;
using Fioo.Entities;
using Fioo.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace Fioo.Tests.Infra;

/// <summary>
/// Sobe a API em memória contra um PostgreSQL real (container descartável),
/// aplicando todas as migrations. Assim as regras do banco (índices únicos,
/// check constraints) também são testadas.
/// </summary>
public sealed class FiooApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string JwtKey = "chave-de-testes-do-fioo-0123456789-abcdefghijklmnop";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    private int _sequencia;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program.cs lê estas configurações na inicialização
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public HttpClient ClienteDe(Usuario usuario)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(usuario));
        return client;
    }

    public async Task<T> NoBanco<T>(Func<AppDbContext, Task<T>> acao)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await acao(db);
    }

    public Task<Usuario> CriarUsuario(UsuarioTipo tipo) => NoBanco(async db =>
    {
        var n = Interlocked.Increment(ref _sequencia);
        var usuario = new Usuario
        {
            Nome = $"{tipo} {n}",
            NomeUsuario = $"{tipo.ToString().ToLowerInvariant()}{n}",
            Email = $"{tipo.ToString().ToLowerInvariant()}{n}@teste.com",
            SenhaHash = "hash",
            Tipo = tipo,
            Ativo = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    });

    public Task<Servico> CriarServico(Usuario fornecedor, ServicoStatus status) => NoBanco(async db =>
    {
        var servico = new Servico
        {
            UsuarioId = fornecedor.Id,
            Titulo = "Costura de jeans",
            TipoCobranca = CobrancaTipo.PorPeca,
            TipoPrazo = PrazoTipo.Semanal,
            Status = status,
            DataCriacao = DateTime.UtcNow
        };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();
        return servico;
    });

    public Task<Candidatura> CriarCandidatura(Servico servico, Usuario costureiro, CandidaturaStatus status = CandidaturaStatus.Pendente) => NoBanco(async db =>
    {
        var candidatura = new Candidatura
        {
            ServicoId = servico.Id,
            UsuarioId = costureiro.Id,
            Status = status,
            DataCandidatura = DateTime.UtcNow
        };
        db.Candidaturas.Add(candidatura);
        await db.SaveChangesAsync();
        return candidatura;
    });

    private static string GerarToken(Usuario usuario)
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            "fioo",
            "fioo",
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim("tipo", usuario.Tipo.ToString())
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

[CollectionDefinition(Nome)]
public class ApiCollection : ICollectionFixture<FiooApiFactory>
{
    public const string Nome = "API";
}
