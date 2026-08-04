using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailContato",
                table: "Usuarios",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeFantasia",
                table: "Usuarios",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "Usuarios",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailContato",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "NomeFantasia",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "Usuarios");
        }
    }
}
