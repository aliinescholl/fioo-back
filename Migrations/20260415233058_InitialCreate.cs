using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Maquinarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maquinarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NomeUsuario = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CpfCnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TelefoneVisivel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    FotoPerfilUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AnosExperiencia = table.Column<int>(type: "integer", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DenuncianteId = table.Column<int>(type: "integer", nullable: false),
                    DenunciadoId = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataDenuncia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncias_Usuarios_DenunciadoId",
                        column: x => x.DenunciadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Denuncias_Usuarios_DenuncianteId",
                        column: x => x.DenuncianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FotoUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataUpload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    TipoCobranca = table.Column<int>(type: "integer", nullable: false),
                    CategoriaServico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    TipoPrazo = table.Column<int>(type: "integer", nullable: true),
                    DataPrazo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioMaquinarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    MaquinarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioMaquinarios", x => new { x.UsuarioId, x.MaquinarioId });
                    table.ForeignKey(
                        name: "FK_UsuarioMaquinarios_Maquinarios_MaquinarioId",
                        column: x => x.MaquinarioId,
                        principalTable: "Maquinarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioMaquinarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Verificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    EtapaEmail = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EtapaDocumento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EtapaTempo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FotoSelfieUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DataEnvioDocumento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataVerificacaoCompleta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verificacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    AvaliadorId = table.Column<int>(type: "integer", nullable: false),
                    AvaliadoId = table.Column<int>(type: "integer", nullable: false),
                    Nota = table.Column<short>(type: "smallint", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataAvaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Usuarios_AvaliadoId",
                        column: x => x.AvaliadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Usuarios_AvaliadorId",
                        column: x => x.AvaliadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Candidaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataCandidatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidaturas_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidaturas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicoMaquinarios",
                columns: table => new
                {
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    MaquinarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicoMaquinarios", x => new { x.ServicoId, x.MaquinarioId });
                    table.ForeignKey(
                        name: "FK_ServicoMaquinarios_Maquinarios_MaquinarioId",
                        column: x => x.MaquinarioId,
                        principalTable: "Maquinarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicoMaquinarios_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadoId",
                table: "Avaliacoes",
                column: "AvaliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_AvaliadorId",
                table: "Avaliacoes",
                column: "AvaliadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId_AvaliadoId",
                table: "Avaliacoes",
                columns: new[] { "ServicoId", "AvaliadorId", "AvaliadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidaturas_ServicoId_UsuarioId",
                table: "Candidaturas",
                columns: new[] { "ServicoId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidaturas_UsuarioId",
                table: "Candidaturas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_DenunciadoId",
                table: "Denuncias",
                column: "DenunciadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_DenuncianteId",
                table: "Denuncias",
                column: "DenuncianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Maquinarios_Nome",
                table: "Maquinarios",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UsuarioId",
                table: "Portfolios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicoMaquinarios_MaquinarioId",
                table: "ServicoMaquinarios",
                column: "MaquinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_UsuarioId",
                table: "Servicos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioMaquinarios_MaquinarioId",
                table: "UsuarioMaquinarios",
                column: "MaquinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NomeUsuario",
                table: "Usuarios",
                column: "NomeUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verificacoes_UsuarioId",
                table: "Verificacoes",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Candidaturas");

            migrationBuilder.DropTable(
                name: "Denuncias");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "ServicoMaquinarios");

            migrationBuilder.DropTable(
                name: "UsuarioMaquinarios");

            migrationBuilder.DropTable(
                name: "Verificacoes");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Maquinarios");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
