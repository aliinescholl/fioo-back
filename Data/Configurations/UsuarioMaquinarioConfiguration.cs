using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class UsuarioMaquinarioConfiguration : IEntityTypeConfiguration<UsuarioMaquinario>
{
    public void Configure(EntityTypeBuilder<UsuarioMaquinario> e)
    {
        e.HasKey(um => new { um.UsuarioId, um.MaquinarioId });

        e.HasOne(um => um.Usuario)
            .WithMany(u => u.Maquinarios)
            .HasForeignKey(um => um.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(um => um.Maquinario)
            .WithMany(m => m.Usuarios)
            .HasForeignKey(um => um.MaquinarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}