using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class DataReferenciaPrazo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataReferenciaPrazo",
                table: "Servicos",
                type: "date",
                nullable: true);

            // Preenche os serviços existentes com a mesma regra de PrazoHelper.CalcularDataReferencia
            migrationBuilder.Sql("""
                UPDATE "Servicos"
                SET "DataReferenciaPrazo" = CASE "TipoPrazo"
                    WHEN 'DataEspecifica' THEN "DataPrazo"
                    WHEN 'Semanal'   THEN ("DataCriacao" AT TIME ZONE 'America/Sao_Paulo')::date + 7
                    WHEN 'Quinzenal' THEN ("DataCriacao" AT TIME ZONE 'America/Sao_Paulo')::date + 15
                    WHEN 'Mensal'    THEN ("DataCriacao" AT TIME ZONE 'America/Sao_Paulo')::date + 30
                    ELSE NULL
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReferenciaPrazo",
                table: "Servicos");
        }
    }
}
