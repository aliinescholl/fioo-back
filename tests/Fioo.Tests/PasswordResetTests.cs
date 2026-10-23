using Fioo.Controller.DTOs;
using Fioo.Controllers;
using Fioo.Data;
using Fioo.Entities;
using Fioo.Enums;
using Fioo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Fioo.Tests;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "fioo-test-webroot");
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "Fioo.Tests";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Testing";
}

public class DummyEmailService : EmailService
{
    public List<(string Para, string Assunto, string Html)> EmailsEnviados { get; } = new();

    public DummyEmailService(IConfiguration config) : base(config, NullLogger<EmailService>.Instance) { }

    public override Task EnviarAsync(string para, string assunto, string html)
    {
        EmailsEnviados.Add((para, assunto, html));
        return Task.CompletedTask;
    }
}

public class PasswordResetTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private (UsuariosController controller, AppDbContext db, DummyEmailService emailService) CreateController(AppDbContext db)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "FrontendUrl", "http://localhost:3000" },
            { "Jwt:Key", "ChaveSuperSecretaParaOsTestesUnitariosDoFioo123456789" },
            { "Jwt:Issuer", "fioo" },
            { "Jwt:Audience", "fioo" }
        }).Build();

        var env = new TestWebHostEnvironment();
        var emailService = new DummyEmailService(config);
        var fotoService = new FotoService(env);
        var cnpjWs = new CnpjWsService(new HttpClient());

        var controller = new UsuariosController(db, config, env, cnpjWs, emailService, fotoService);
        return (controller, db, emailService);
    }

    private static string Hash(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
    }

    [Fact]
    public async Task EmailInexistente_DevolveMesmaRespostaDeEmailExistente()
    {
        using var db = CreateDbContext();
        var (controller, _, emailService) = CreateController(db);

        // Usuário existente
        db.Usuarios.Add(new Usuario
        {
            Nome = "Usuario Teste",
            NomeUsuario = "teste",
            Email = "cadastrado@fioo.com",
            SenhaHash = Hash("Senha@123"),
            Tipo = UsuarioTipo.Costureiro,
            Ativo = true
        });
        await db.SaveChangesAsync();

        // 1. Chamada com e-mail existente
        var resultExistente = await controller.EsqueciSenha(new EsqueciSenhaDto { Email = "cadastrado@fioo.com" }) as OkObjectResult;
        Assert.NotNull(resultExistente);

        // 2. Chamada com e-mail inexistente
        var resultInexistente = await controller.EsqueciSenha(new EsqueciSenhaDto { Email = "inexistente@fioo.com" }) as OkObjectResult;
        Assert.NotNull(resultInexistente);

        // Valida que ambas as respostas têm a mesma mensagem
        var jsonExistente = System.Text.Json.JsonSerializer.Serialize(resultExistente.Value);
        var jsonInexistente = System.Text.Json.JsonSerializer.Serialize(resultInexistente.Value);

        Assert.Equal(jsonExistente, jsonInexistente);
        // Apenas o usuário existente deve ter gerado envio de e-mail
        Assert.Single(emailService.EmailsEnviados);
        Assert.Equal("cadastrado@fioo.com", emailService.EmailsEnviados[0].Para);
    }

    [Fact]
    public async Task TokenValido_RedefineSenhaComSucesso()
    {
        using var db = CreateDbContext();
        var (controller, _, _) = CreateController(db);

        var usuario = new Usuario
        {
            Nome = "Usuario Token Valido",
            NomeUsuario = "tokenvalido",
            Email = "valido@fioo.com",
            SenhaHash = Hash("SenhaAntiga@123"),
            Tipo = UsuarioTipo.Costureiro,
            Ativo = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var tokenPuro = "token-seguro-123456789";
        var tokenHash = Hash(tokenPuro);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenHash,
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = null
        });
        await db.SaveChangesAsync();

        var result = await controller.RedefinirSenha(new RedefinirSenhaDto
        {
            Token = tokenPuro,
            NovaSenha = "NovaSenhaForte@999"
        }) as OkObjectResult;

        Assert.NotNull(result);

        // Verifica que o hash no banco foi alterado
        var usuarioAtualizado = await db.Usuarios.FindAsync(usuario.Id);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal(Hash("NovaSenhaForte@999"), usuarioAtualizado.SenhaHash);

        // Verifica que o token foi marcado como usado
        var tokenRegistrado = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        Assert.NotNull(tokenRegistrado);
        Assert.NotNull(tokenRegistrado.UsadoEm);
    }

    [Fact]
    public async Task TokenExpirado_ERejeitado()
    {
        using var db = CreateDbContext();
        var (controller, _, _) = CreateController(db);

        var usuario = new Usuario
        {
            Nome = "Usuario Expirado",
            NomeUsuario = "expirado",
            Email = "expirado@fioo.com",
            SenhaHash = Hash("SenhaAntiga@123"),
            Tipo = UsuarioTipo.Costureiro,
            Ativo = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var tokenPuro = "token-expirado-123";
        var tokenHash = Hash(tokenPuro);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenHash,
            ExpiraEm = DateTime.UtcNow.AddMinutes(-5), // Já expirado
            UsadoEm = null
        });
        await db.SaveChangesAsync();

        var result = await controller.RedefinirSenha(new RedefinirSenhaDto
        {
            Token = tokenPuro,
            NovaSenha = "NovaSenhaForte@999"
        }) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal("Token inválido ou expirado.", result.Value);

        // Senha não pode ter sido alterada
        var usuarioAtualizado = await db.Usuarios.FindAsync(usuario.Id);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal(Hash("SenhaAntiga@123"), usuarioAtualizado.SenhaHash);
    }

    [Fact]
    public async Task TokenJaUsado_ERejeitado()
    {
        using var db = CreateDbContext();
        var (controller, _, _) = CreateController(db);

        var usuario = new Usuario
        {
            Nome = "Usuario Ja Usado",
            NomeUsuario = "jausado",
            Email = "jausado@fioo.com",
            SenhaHash = Hash("SenhaAntiga@123"),
            Tipo = UsuarioTipo.Costureiro,
            Ativo = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var tokenPuro = "token-jausado-123";
        var tokenHash = Hash(tokenPuro);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenHash,
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = DateTime.UtcNow.AddMinutes(-10) // Já foi usado
        });
        await db.SaveChangesAsync();

        var result = await controller.RedefinirSenha(new RedefinirSenhaDto
        {
            Token = tokenPuro,
            NovaSenha = "NovaSenhaForte@999"
        }) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal("Token inválido ou expirado.", result.Value);

        // Senha não pode ter sido alterada
        var usuarioAtualizado = await db.Usuarios.FindAsync(usuario.Id);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal(Hash("SenhaAntiga@123"), usuarioAtualizado.SenhaHash);
    }
}
