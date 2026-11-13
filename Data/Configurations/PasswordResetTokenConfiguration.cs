using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityAlwaysColumn();

        builder.Property(t => t.TokenHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash);

        builder.Property(t => t.ExpiraEm)
            .IsRequired();

        builder.Property(t => t.DataCriacao)
            .HasDefaultValueSql("now()");

        builder.HasOne(t => t.Usuario)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
