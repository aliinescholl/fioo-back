using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class VerificacaoConfiguration : IEntityTypeConfiguration<Verificacao>
{
    public void Configure(EntityTypeBuilder<Verificacao> e)
    {
        e.HasKey(v => v.Id);
        e.Property(v => v.Id).UseIdentityAlwaysColumn();

        e.Property(v => v.UsuarioId).IsRequired();

        e.HasIndex(v => v.UsuarioId).IsUnique();

        e.Property(v => v.EtapaEmail)
            .HasDefaultValue(false);

        e.Property(v => v.EtapaDocumento)
            .HasDefaultValue(false);

        e.Property(v => v.EtapaTempo)
            .HasDefaultValue(false);

        e.Property(v => v.FotoSelfieUrl)
            .HasMaxLength(512);

        e.HasOne(v => v.Usuario)
            .WithOne(u => u.Verificacao)
            .HasForeignKey<Verificacao>(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}