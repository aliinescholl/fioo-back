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

        e.ToTable(t => t.HasCheckConstraint(
            "CK_Servicos_Status",
            "\"Status\" IN ('EmAndamento', 'Concluido', 'Cancelado')"));

        e.Property(s => s.DataCriacao)
            .HasDefaultValueSql("now()");

        // Índices usados pela listagem de serviços (filtros e ordenação):
        // ordenação padrão "Mais relevantes" (status, depois mais recentes)
        e.HasIndex(s => new { s.Status, s.DataCriacao });
        // ordenação por prazo mais próximo/distante
        e.HasIndex(s => s.DataReferenciaPrazo);
        // faixa de valor e ordenação por maior/menor valor
        e.HasIndex(s => s.Valor);

        e.HasOne(s => s.Usuario)
            .WithMany(u => u.Servicos)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}