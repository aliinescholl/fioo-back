using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class FiltrosServicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza texto para buscas sem diferenciar maiúsculas/minúsculas e acentos (pt-BR),
            // sem depender da extensão unaccent. IMMUTABLE para poder ser usada em índices.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION normalizar_texto(texto text) RETURNS text
                LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE AS $$
                    SELECT lower(translate(btrim(texto),
                        'ÁÀÂÃÄÅáàâãäåÉÈÊËéèêëÍÌÎÏíìîïÓÒÔÕÖóòôõöÚÙÛÜúùûüÇçÑñÝýÿ',
                        'AAAAAAaaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUUuuuuCcNnYyy'));
                $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_DataReferenciaPrazo",
                table: "Servicos",
                column: "DataReferenciaPrazo");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Status_DataCriacao",
                table: "Servicos",
                columns: new[] { "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Valor",
                table: "Servicos",
                column: "Valor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servicos_DataReferenciaPrazo",
                table: "Servicos");

            migrationBuilder.DropIndex(
                name: "IX_Servicos_Status_DataCriacao",
                table: "Servicos");

            migrationBuilder.DropIndex(
                name: "IX_Servicos_Valor",
                table: "Servicos");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS normalizar_texto(text);");
        }
    }
}
