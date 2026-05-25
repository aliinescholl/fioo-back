using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class DenunciaConfiguration : IEntityTypeConfiguration<Denuncia>
{
    public void Configure(EntityTypeBuilder<Denuncia> e)
    {
        e.HasKey(d => d.Id);
        e.Property(d => d.Id).UseIdentityAlwaysColumn();

        e.Property(d => d.DenuncianteId).IsRequired();
        e.Property(d => d.DenunciadoId).IsRequired();

        e.Property(d => d.Motivo)
            .HasMaxLength(150)
            .IsRequired();

        e.Property(d => d.Descricao)
            .HasMaxLength(2000);

        e.Property(d => d.Status)
            .HasConversion<string>();

        e.Property(d => d.DataDenuncia)
            .HasDefaultValueSql("now()");

        e.HasOne(d => d.Denunciado)
            .WithMany()
            .HasForeignKey(d => d.DenunciadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}