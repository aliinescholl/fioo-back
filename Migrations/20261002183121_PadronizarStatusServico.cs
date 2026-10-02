using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarStatusServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ativo → Em andamento, Em Andamento → Em andamento, Finalizado → Concluído, Cancelado → Cancelado
            migrationBuilder.Sql("""
                UPDATE "Servicos" SET "Status" = 'EmAndamento' WHERE "Status" = 'Ativo';
                UPDATE "Servicos" SET "Status" = 'Concluido' WHERE "Status" = 'Finalizado';
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Servicos_Status",
                table: "Servicos",
                sql: "\"Status\" IN ('EmAndamento', 'Concluido', 'Cancelado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Servicos_Status",
                table: "Servicos");

            // Volta aos valores antigos. "Ativo" e "Em Andamento" não podem ser separados de novo:
            // todos voltam como "Ativo" para continuarem visíveis na listagem da versão anterior.
            migrationBuilder.Sql("""
                UPDATE "Servicos" SET "Status" = 'Ativo' WHERE "Status" = 'EmAndamento';
                UPDATE "Servicos" SET "Status" = 'Finalizado' WHERE "Status" = 'Concluido';
                """);
        }
    }
}
