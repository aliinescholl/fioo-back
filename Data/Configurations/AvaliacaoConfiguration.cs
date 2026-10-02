using Fioo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fioo.Data.Configurations;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public const int ComentarioMaxLength = 300;

    public void Configure(EntityTypeBuilder<Avaliacao> e)
    {
        e.HasKey(a => a.Id);
        e.Property(a => a.Id).UseIdentityAlwaysColumn();

        e.Property(a => a.ServicoId).IsRequired();
        e.Property(a => a.AvaliadorId).IsRequired();
        e.Property(a => a.AvaliadoId).IsRequired();

        // Uma avaliação por participante por serviço
        e.HasIndex(a => new { a.ServicoId, a.AvaliadorId }).IsUnique();

        // Média e total por usuário e papel (perfil, cards e ordenação da tela Encontrar)
        e.HasIndex(a => new { a.AvaliadoId, a.PapelAvaliado });

        e.Property(a => a.PapelAvaliado)
            .HasConversion<string>()
            .IsRequired();

        e.Property(a => a.Nota).IsRequired();
        e.Property(a => a.NotaComunicacao).IsRequired();
        e.Property(a => a.NotaQualidade).IsRequired();

        e.Property(a => a.Comentario)
            .HasMaxLength(ComentarioMaxLength);

        e.Property(a => a.DataAvaliacao)
            .HasDefaultValueSql("now()");

        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Avaliacoes_Nota", "\"Nota\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Avaliacoes_NotaComunicacao", "\"NotaComunicacao\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Avaliacoes_NotaQualidade", "\"NotaQualidade\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Avaliacoes_PapelAvaliado", "\"PapelAvaliado\" IN ('Costureiro', 'Fornecedor')");
            t.HasCheckConstraint("CK_Avaliacoes_NaoAutoAvaliacao", "\"AvaliadorId\" <> \"AvaliadoId\"");
        });

        // Avaliações não somem junto com o serviço: excluir serviço avaliado é bloqueado
        e.HasOne(a => a.Servico)
            .WithMany(s => s.Avaliacoes)
            .HasForeignKey(a => a.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
