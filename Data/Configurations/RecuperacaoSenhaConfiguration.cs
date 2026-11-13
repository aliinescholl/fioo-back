using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class RecuperacaoSenhaConfiguration : IEntityTypeConfiguration<RecuperacaoSenha>
{
    public void Configure(EntityTypeBuilder<RecuperacaoSenha> e)
    {
        e.ToTable("RecuperacoesSenha");
        e.HasKey(r => r.Id);
        e.Property(r => r.Id).UseIdentityAlwaysColumn();

        e.Property(r => r.TokenHash).IsRequired().HasMaxLength(64);
        e.HasIndex(r => r.TokenHash).IsUnique();
        e.HasIndex(r => r.UsuarioId);

        e.Property(r => r.DataCriacao).IsRequired();
        e.Property(r => r.DataExpiracao).IsRequired();

        e.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
