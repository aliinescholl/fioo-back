using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> e)
    {
        e.HasKey(p => p.Id);
        e.Property(p => p.Id).UseIdentityAlwaysColumn();

        e.Property(p => p.UsuarioId).IsRequired();

        e.Property(p => p.FotoUrl)
            .HasMaxLength(512)
            .IsRequired();

        e.Property(p => p.Descricao)
            .HasMaxLength(500);

        e.Property(p => p.DataUpload)
            .HasDefaultValueSql("now()");

        e.HasOne(p => p.Usuario)
            .WithMany(u => u.Portfolios)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}