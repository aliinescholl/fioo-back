using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> e)
    {
        e.HasKey(s => s.Id);
        e.Property(s => s.Id).UseIdentityAlwaysColumn();

        e.Property(s => s.UsuarioId).IsRequired();

        e.Property(s => s.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        e.Property(s => s.Cidade)
            .HasMaxLength(100);

        e.Property(s => s.Estado)
            .HasMaxLength(2);

        e.Property(s => s.TipoCobranca)
            .HasConversion<string>();

        e.Property(s => s.CategoriaServico)
            .HasMaxLength(100);

        e.Property(s => s.Valor)
            .HasColumnType("numeric(12,2)");

        e.Property(s => s.TipoPrazo)
            .HasConversion<string>();

        e.Property(s => s.Status)
            .HasConversion<string>();

        e.Property(s => s.DataCriacao)
            .HasDefaultValueSql("now()");

        e.HasOne(s => s.Usuario)
            .WithMany(u => u.Servicos)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}