using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> e)
    {
        e.HasKey(a => a.Id);
        e.Property(a => a.Id).UseIdentityAlwaysColumn();

        e.Property(a => a.ServicoId).IsRequired();
        e.Property(a => a.AvaliadorId).IsRequired();
        e.Property(a => a.AvaliadoId).IsRequired();

        e.HasIndex(a => new { a.ServicoId, a.AvaliadorId, a.AvaliadoId }).IsUnique();

        e.Property(a => a.Nota).IsRequired();

        e.Property(a => a.Comentario)
            .HasMaxLength(1000);

        e.Property(a => a.DataAvaliacao)
            .HasDefaultValueSql("now()");

        e.HasOne(a => a.Servico)
            .WithMany(s => s.Avaliacoes)
            .HasForeignKey(a => a.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}