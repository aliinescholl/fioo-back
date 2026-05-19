using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class MaquinarioConfiguration : IEntityTypeConfiguration<Maquinario>
{
    public void Configure(EntityTypeBuilder<Maquinario> e)
    {
        e.HasKey(m => m.Id);
        e.Property(m => m.Id).UseIdentityAlwaysColumn();

        e.Property(m => m.Nome)
            .HasMaxLength(150)
            .IsRequired();

        e.HasIndex(m => m.Nome).IsUnique();
    }
}