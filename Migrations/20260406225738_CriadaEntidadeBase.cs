using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class CriadaEntidadeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "maquinarios");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "avaliacoes");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "usuarios",
                newName: "Nome");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "candidaturas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "candidaturas");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "usuarios",
                newName: "nome");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "maquinarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "avaliacoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
