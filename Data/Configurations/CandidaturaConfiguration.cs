using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class CandidaturaConfiguration : IEntityTypeConfiguration<Candidatura>
{
    public void Configure(EntityTypeBuilder<Candidatura> e)
    {
        e.HasKey(c => c.Id);
        e.Property(c => c.Id).UseIdentityAlwaysColumn();

        e.Property(c => c.ServicoId).IsRequired();
        e.Property(c => c.UsuarioId).IsRequired();

        e.HasIndex(c => new { c.ServicoId, c.UsuarioId }).IsUnique();

        // Vínculo do costureiro com o serviço: no máximo uma candidatura aceita por serviço
        e.HasIndex(c => c.ServicoId, "IX_Candidaturas_ServicoId_Aceita")
            .IsUnique()
            .HasFilter("\"Status\" = 'Aceita'");

        e.Property(c => c.Status)
            .HasConversion<string>();

        e.Property(c => c.DataCandidatura)
            .HasDefaultValueSql("now()");

        e.HasOne(c => c.Servico)
            .WithMany(s => s.Candidaturas)
            .HasForeignKey(c => c.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(c => c.Usuario)
            .WithMany(u => u.Candidaturas)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}