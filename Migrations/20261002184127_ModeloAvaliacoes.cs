using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class ModeloAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Não corta nem apaga dados: se alguma avaliação existente violar as novas regras,
            // a migration é interrompida para correção manual.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Avaliacoes" WHERE length("Comentario") > 300) THEN
                        RAISE EXCEPTION 'Existem avaliações com comentário acima de 300 caracteres. Corrija antes de aplicar a migration ModeloAvaliacoes.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Avaliacoes" WHERE "Nota" NOT BETWEEN 1 AND 5) THEN
                        RAISE EXCEPTION 'Existem avaliações com nota fora de 1 a 5. Corrija antes de aplicar a migration ModeloAvaliacoes.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Avaliacoes" GROUP BY "ServicoId", "AvaliadorId" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Existem avaliações duplicadas do mesmo avaliador no mesmo serviço. Corrija antes de aplicar a migration ModeloAvaliacoes.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Avaliacoes" WHERE "AvaliadorId" = "AvaliadoId") THEN
                        RAISE EXCEPTION 'Existem autoavaliações. Corrija antes de aplicar a migration ModeloAvaliacoes.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_AvaliadoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId_AvaliadoId",
                table: "Avaliacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Comentario",
                table: "Avaliacoes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "NotaComunicacao",
                table: "Avaliacoes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "NotaQualidade",
                table: "Avaliacoes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "PapelAvaliado",
                table: "Avaliacoes",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Avaliações existentes: aspectos recebem a nota geral; o papel vem do serviço
            // (o dono do serviço é o fornecedor; o outro participante, o costureiro)
            migrationBuilder.Sql("""
                UPDATE "Avaliacoes" a
                SET "NotaComunicacao" = a."Nota",
                    "NotaQualidade" = a."Nota",
                    "PapelAvaliado" = CASE WHEN a."AvaliadoId" = s."UsuarioId" THEN 'Fornecedor' ELSE 'Costureiro' END
                FROM "Servicos" s
                WHERE s."Id" = a."ServicoId";

                ALTER TABLE "Avaliacoes" ALTER COLUMN "NotaComunicacao" DROP DEFAULT;
                ALTER TABLE "Avaliacoes" ALTER COLUMN "NotaQualidade" DROP DEFAULT;
                ALTER TABLE "Avaliacoes" ALTER COLUMN "PapelAvaliado" DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadoId_PapelAvaliado",
                table: "Avaliacoes",
                columns: new[] { "AvaliadoId", "PapelAvaliado" });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId",
                table: "Avaliacoes",
                columns: new[] { "ServicoId", "AvaliadorId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacoes_NaoAutoAvaliacao",
                table: "Avaliacoes",
                sql: "\"AvaliadorId\" <> \"AvaliadoId\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacoes_Nota",
                table: "Avaliacoes",
                sql: "\"Nota\" BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacoes_NotaComunicacao",
                table: "Avaliacoes",
                sql: "\"NotaComunicacao\" BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacoes_NotaQualidade",
                table: "Avaliacoes",
                sql: "\"NotaQualidade\" BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacoes_PapelAvaliado",
                table: "Avaliacoes",
                sql: "\"PapelAvaliado\" IN ('Costureiro', 'Fornecedor')");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_AvaliadoId_PapelAvaliado",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId",
                table: "Avaliacoes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacoes_NaoAutoAvaliacao",
                table: "Avaliacoes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacoes_Nota",
                table: "Avaliacoes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacoes_NotaComunicacao",
                table: "Avaliacoes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacoes_NotaQualidade",
                table: "Avaliacoes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacoes_PapelAvaliado",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "NotaComunicacao",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "NotaQualidade",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "PapelAvaliado",
                table: "Avaliacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Comentario",
                table: "Avaliacoes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadoId",
                table: "Avaliacoes",
                column: "AvaliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId_AvaliadoId",
                table: "Avaliacoes",
                columns: new[] { "ServicoId", "AvaliadorId", "AvaliadoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
