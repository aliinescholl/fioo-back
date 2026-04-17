using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class ServicoMaquinarioConfiguration : IEntityTypeConfiguration<ServicoMaquinario>
{
    public void Configure(EntityTypeBuilder<ServicoMaquinario> e)
    {
        e.HasKey(sm => new { sm.ServicoId, sm.MaquinarioId });

        e.HasOne(sm => sm.Servico)
            .WithMany(s => s.Maquinarios)
            .HasForeignKey(sm => sm.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(sm => sm.Maquinario)
            .WithMany(m => m.Servicos)
            .HasForeignKey(sm => sm.MaquinarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}