using Fioo.Entities;
using Fioo.Enums;
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

        modelBuilder.HasPostgresEnum<UsuarioTipo>();
        modelBuilder.HasPostgresEnum<ServicoStatus>();
        modelBuilder.HasPostgresEnum<CandidaturaStatus>();
        modelBuilder.HasPostgresEnum<CobrancaTipo>();
        modelBuilder.HasPostgresEnum<PrazoTipo>();
        modelBuilder.HasPostgresEnum<DenunciaStatus>();

        ConfigureUsuario(modelBuilder);
        ConfigureServico(modelBuilder);
        ConfigureCandidatura(modelBuilder);
        ConfigureAvaliacao(modelBuilder);
        ConfigureDenuncia(modelBuilder);
        ConfigurePortfolio(modelBuilder);
        ConfigureMaquinario(modelBuilder);
        ConfigureUsuarioMaquinario(modelBuilder);
        ConfigureServicoMaquinario(modelBuilder);
        ConfigureVerificacao(modelBuilder);
    }


    private static void ConfigureUsuario(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");

            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(u => u.Nome)
                .HasColumnName("nome")
                .HasMaxLength(150)
                .IsRequired();

            e.Property(u => u.NomeUsuario)
                .HasColumnName("nome_usuario")
                .HasMaxLength(60)
                .IsRequired();

            e.HasIndex(u => u.NomeUsuario).IsUnique();

            e.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();

            e.HasIndex(u => u.Email).IsUnique();

            e.Property(u => u.SenhaHash)
                .HasColumnName("senha_hash")
                .HasMaxLength(255)
                .IsRequired();

            e.Property(u => u.CpfCnpj)
                .HasColumnName("cpf_cnpj")
                .HasMaxLength(18);

            e.Property(u => u.Telefone)
                .HasColumnName("telefone")
                .HasMaxLength(20);

            e.Property(u => u.TelefoneVisivel)
                .HasColumnName("telefone_visivel")
                .HasDefaultValue(false);

            e.Property(u => u.Cidade)
                .HasColumnName("cidade")
                .HasMaxLength(100);

            e.Property(u => u.Estado)
                .HasColumnName("estado")
                .HasMaxLength(2);

            e.Property(u => u.Tipo)
                .HasColumnName("tipo")
                .HasConversion<string>();

            e.Property(u => u.FotoPerfilUrl)
                .HasColumnName("foto_perfil_url")
                .HasMaxLength(512);

            e.Property(u => u.AnosExperiencia)
                .HasColumnName("anos_experiencia");

            e.Property(u => u.DataCadastro)
                .HasColumnName("data_cadastro")
                .HasDefaultValueSql("now()");

            e.Property(u => u.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            // Navegações com múltiplos relacionamentos para Usuario
            e.HasMany(u => u.AvaliacoesFeitas)
                .WithOne(a => a.Avaliador)
                .HasForeignKey(a => a.AvaliadorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(u => u.AvaliacoesRecebidas)
                .WithOne(a => a.Avaliado)
                .HasForeignKey(a => a.AvaliadoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(u => u.DenunciasFeitas)
                .WithOne(d => d.Denunciante)
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }


    private static void ConfigureServico(ModelBuilder mb)
    {
        mb.Entity<Servico>(e =>
        {
            e.ToTable("servicos");

            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(s => s.UsuarioId).HasColumnName("usuario_id").IsRequired();

            e.Property(s => s.Titulo)
                .HasColumnName("titulo")
                .HasMaxLength(200)
                .IsRequired();

            e.Property(s => s.Descricao)
                .HasColumnName("descricao");

            e.Property(s => s.Cidade)
                .HasColumnName("cidade")
                .HasMaxLength(100);

            e.Property(s => s.Estado)
                .HasColumnName("estado")
                .HasMaxLength(2);

            e.Property(s => s.TipoCobranca)
                .HasColumnName("tipo_cobranca")
                .HasConversion<string>();

            e.Property(s => s.CategoriaServico)
                .HasColumnName("categoria_servico")
                .HasMaxLength(100);

            e.Property(s => s.Valor)
                .HasColumnName("valor")
                .HasColumnType("numeric(12,2)");

            e.Property(s => s.TipoPrazo)
                .HasColumnName("tipo_prazo")
                .HasConversion<string>();

            e.Property(s => s.DataPrazo)
                .HasColumnName("data_prazo");

            e.Property(s => s.Status)
                .HasColumnName("status")
                .HasConversion<string>();

            e.Property(s => s.DataCriacao)
                .HasColumnName("data_criacao")
                .HasDefaultValueSql("now()");

            e.HasOne(s => s.Usuario)
                .WithMany(u => u.Servicos)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCandidatura(ModelBuilder mb)
    {
        mb.Entity<Candidatura>(e =>
        {
            e.ToTable("candidaturas");

            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(c => c.ServicoId).HasColumnName("servico_id").IsRequired();
            e.Property(c => c.UsuarioId).HasColumnName("usuario_id").IsRequired();

            // Evita candidatura duplicada do mesmo usuário no mesmo serviço
            e.HasIndex(c => new { c.ServicoId, c.UsuarioId }).IsUnique();

            e.Property(c => c.Status)
                .HasColumnName("status")
                .HasConversion<string>();

            e.Property(c => c.DataCandidatura)
                .HasColumnName("data_candidatura")
                .HasDefaultValueSql("now()");

            e.Property(c => c.DataAtualizacao)
                .HasColumnName("data_atualizacao");

            e.HasOne(c => c.Servico)
                .WithMany(s => s.Candidaturas)
                .HasForeignKey(c => c.ServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Usuario)
                .WithMany(u => u.Candidaturas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAvaliacao(ModelBuilder mb)
    {
        mb.Entity<Avaliacao>(e =>
        {
            e.ToTable("avaliacoes");

            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(a => a.ServicoId).HasColumnName("servico_id").IsRequired();
            e.Property(a => a.AvaliadorId).HasColumnName("avaliador_id").IsRequired();
            e.Property(a => a.AvaliadoId).HasColumnName("avaliado_id").IsRequired();

            e.HasIndex(a => new { a.ServicoId, a.AvaliadorId, a.AvaliadoId }).IsUnique();

            e.Property(a => a.Nota)
                .HasColumnName("nota")
                .IsRequired();

            e.Property(a => a.Comentario)
                .HasColumnName("comentario")
                .HasMaxLength(1000);

            e.Property(a => a.DataAvaliacao)
                .HasColumnName("data_avaliacao")
                .HasDefaultValueSql("now()");

            e.HasOne(a => a.Servico)
                .WithMany(s => s.Avaliacoes)
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Cascade);

        });
    }

    private static void ConfigureDenuncia(ModelBuilder mb)
    {
        mb.Entity<Denuncia>(e =>
        {
            e.ToTable("denuncias");

            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(d => d.DenuncianteId).HasColumnName("denunciante_id").IsRequired();
            e.Property(d => d.DenunciadoId).HasColumnName("denunciado_id").IsRequired();

            e.Property(d => d.Motivo)
                .HasColumnName("motivo")
                .HasMaxLength(150)
                .IsRequired();

            e.Property(d => d.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(2000);

            e.Property(d => d.Status)
                .HasColumnName("status")
                .HasConversion<string>();

            e.Property(d => d.DataDenuncia)
                .HasColumnName("data_denuncia")
                .HasDefaultValueSql("now()");

            e.HasOne(d => d.Denunciado)
                .WithMany()
                .HasForeignKey(d => d.DenunciadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePortfolio(ModelBuilder mb)
    {
        mb.Entity<Portfolio>(e =>
        {
            e.ToTable("portfolios");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(p => p.UsuarioId).HasColumnName("usuario_id").IsRequired();

            e.Property(p => p.FotoUrl)
                .HasColumnName("foto_url")
                .HasMaxLength(512)
                .IsRequired();

            e.Property(p => p.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(500);

            e.Property(p => p.DataUpload)
                .HasColumnName("data_upload")
                .HasDefaultValueSql("now()");

            e.HasOne(p => p.Usuario)
                .WithMany(u => u.Portfolios)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMaquinario(ModelBuilder mb)
    {
        mb.Entity<Maquinario>(e =>
        {
            e.ToTable("maquinarios");

            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(m => m.Nome)
                .HasColumnName("nome")
                .HasMaxLength(150)
                .IsRequired();

            e.HasIndex(m => m.Nome).IsUnique();
        });
    }

    private static void ConfigureUsuarioMaquinario(ModelBuilder mb)
    {
        mb.Entity<UsuarioMaquinario>(e =>
        {
            e.ToTable("usuario_maquinarios");

            e.HasKey(um => new { um.UsuarioId, um.MaquinarioId });

            e.Property(um => um.UsuarioId).HasColumnName("usuario_id");
            e.Property(um => um.MaquinarioId).HasColumnName("maquinario_id");

            e.HasOne(um => um.Usuario)
                .WithMany(u => u.Maquinarios)
                .HasForeignKey(um => um.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(um => um.Maquinario)
                .WithMany(m => m.Usuarios)
                .HasForeignKey(um => um.MaquinarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureServicoMaquinario(ModelBuilder mb)
    {
        mb.Entity<ServicoMaquinario>(e =>
        {
            e.ToTable("servico_maquinarios");

            e.HasKey(sm => new { sm.ServicoId, sm.MaquinarioId });

            e.Property(sm => sm.ServicoId).HasColumnName("servico_id");
            e.Property(sm => sm.MaquinarioId).HasColumnName("maquinario_id");

            e.HasOne(sm => sm.Servico)
                .WithMany(s => s.Maquinarios)
                .HasForeignKey(sm => sm.ServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(sm => sm.Maquinario)
                .WithMany(m => m.Servicos)
                .HasForeignKey(sm => sm.MaquinarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVerificacao(ModelBuilder mb)
    {
        mb.Entity<Verificacao>(e =>
        {
            e.ToTable("verificacoes");

            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasColumnName("id").UseIdentityAlwaysColumn();

            e.Property(v => v.UsuarioId).HasColumnName("usuario_id").IsRequired();

            e.HasIndex(v => v.UsuarioId).IsUnique(); 

            e.Property(v => v.EtapaEmail)
                .HasColumnName("etapa_email")
                .HasDefaultValue(false);

            e.Property(v => v.EtapaDocumento)
                .HasColumnName("etapa_documento")
                .HasDefaultValue(false);

            e.Property(v => v.EtapaTempo)
                .HasColumnName("etapa_tempo")
                .HasDefaultValue(false);

            e.Property(v => v.FotoSelfieUrl)
                .HasColumnName("foto_selfie_url")
                .HasMaxLength(512);

            e.Property(v => v.DataEnvioDocumento)
                .HasColumnName("data_envio_documento");

            e.Property(v => v.DataVerificacaoCompleta)
                .HasColumnName("data_verificacao_completa");

            e.HasOne(v => v.Usuario)
                .WithOne(u => u.Verificacao)
                .HasForeignKey<Verificacao>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}