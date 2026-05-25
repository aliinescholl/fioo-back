using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.HasKey(u => u.Id);
        e.Property(u => u.Id).UseIdentityAlwaysColumn();

        e.Property(u => u.Nome)
            .HasMaxLength(150)
            .IsRequired();

        e.Property(u => u.NomeUsuario)
            .HasMaxLength(60)
            .IsRequired();

        e.HasIndex(u => u.NomeUsuario).IsUnique();

        e.Property(u => u.Email)
            .HasMaxLength(254)
            .IsRequired();

        e.HasIndex(u => u.Email).IsUnique();

        e.Property(u => u.SenhaHash)
            .HasMaxLength(255)
            .IsRequired();

        e.Property(u => u.CpfCnpj)
            .HasMaxLength(18);

        e.Property(u => u.Telefone)
            .HasMaxLength(20);

        e.Property(u => u.TelefoneVisivel)
            .HasDefaultValue(false);

        e.Property(u => u.Estado)
            .HasMaxLength(2);

        e.Property(u => u.Cidade)
            .HasMaxLength(100);

        e.Property(u => u.Tipo)
            .HasConversion<string>();

        e.Property(u => u.FotoPerfilUrl)
            .HasMaxLength(512);

        e.Property(u => u.DataCadastro)
            .HasDefaultValueSql("now()");

        e.Property(u => u.Ativo)
            .HasDefaultValue(true);

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
    }
}