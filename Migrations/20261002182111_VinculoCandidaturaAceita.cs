using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class VinculoCandidaturaAceita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Não altera dados: se já houver serviço com mais de uma candidatura aceita,
            // interrompe a migration para que os dados sejam corrigidos manualmente.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Candidaturas"
                        WHERE "Status" = 'Aceita'
                        GROUP BY "ServicoId"
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Existem serviços com mais de uma candidatura aceita. Corrija os dados antes de aplicar a migration VinculoCandidaturaAceita.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Candidaturas_ServicoId_Aceita",
                table: "Candidaturas",
                column: "ServicoId",
                unique: true,
                filter: "\"Status\" = 'Aceita'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Candidaturas_ServicoId_Aceita",
                table: "Candidaturas");
        }
    }
}
