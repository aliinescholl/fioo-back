using Fioo.Data.Configurations;
using Fioo.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fioo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Candidatura> Candidaturas => Set<Candidatura>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<Denuncia> Denuncias => Set<Denuncia>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Maquinario> Maquinarios => Set<Maquinario>();
    public DbSet<UsuarioMaquinario> UsuarioMaquinarios => Set<UsuarioMaquinario>();
    public DbSet<ServicoMaquinario> ServicoMaquinarios => Set<ServicoMaquinario>();
    public DbSet<Verificacao> Verificacoes => Set<Verificacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}