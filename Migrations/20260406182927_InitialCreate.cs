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
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:candidatura_status", "pendente,aceita,recusada,cancelada")
                .Annotation("Npgsql:Enum:cobranca_tipo", "por_peca,por_operacao")
                .Annotation("Npgsql:Enum:denuncia_status", "aberta,em_analise,resolvida,arquivada")
                .Annotation("Npgsql:Enum:prazo_tipo", "semanal,quinzenal,mensal,data_especifica")
                .Annotation("Npgsql:Enum:servico_status", "ativo,em_andamento,finalizado,cancelado")
                .Annotation("Npgsql:Enum:usuario_tipo", "costureiro,fornecedor");

            migrationBuilder.CreateTable(
                name: "maquinarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maquinarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome_usuario = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cpf_cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telefone_visivel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    foto_perfil_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    anos_experiencia = table.Column<int>(type: "integer", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "denuncias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    denunciante_id = table.Column<int>(type: "integer", nullable: false),
                    denunciado_id = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_denuncia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_denuncias", x => x.id);
                    table.ForeignKey(
                        name: "FK_denuncias_usuarios_denunciado_id",
                        column: x => x.denunciado_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_denuncias_usuarios_denunciante_id",
                        column: x => x.denunciante_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    foto_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "FK_portfolios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    tipo_cobranca = table.Column<string>(type: "text", nullable: false),
                    categoria_servico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    tipo_prazo = table.Column<string>(type: "text", nullable: true),
                    data_prazo = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_servicos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_maquinarios",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    maquinario_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_maquinarios", x => new { x.usuario_id, x.maquinario_id });
                    table.ForeignKey(
                        name: "FK_usuario_maquinarios_maquinarios_maquinario_id",
                        column: x => x.maquinario_id,
                        principalTable: "maquinarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_maquinarios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verificacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    etapa_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    etapa_documento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    etapa_tempo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    foto_selfie_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    data_envio_documento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_verificacao_completa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verificacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_verificacoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    servico_id = table.Column<int>(type: "integer", nullable: false),
                    avaliador_id = table.Column<int>(type: "integer", nullable: false),
                    avaliado_id = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<short>(type: "smallint", nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_avaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_avaliacoes_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avaliacoes_usuarios_avaliado_id",
                        column: x => x.avaliado_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_avaliacoes_usuarios_avaliador_id",
                        column: x => x.avaliador_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "candidaturas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    servico_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_candidatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidaturas", x => x.id);
                    table.ForeignKey(
                        name: "FK_candidaturas_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_candidaturas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servico_maquinarios",
                columns: table => new
                {
                    servico_id = table.Column<int>(type: "integer", nullable: false),
                    maquinario_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servico_maquinarios", x => new { x.servico_id, x.maquinario_id });
                    table.ForeignKey(
                        name: "FK_servico_maquinarios_maquinarios_maquinario_id",
                        column: x => x.maquinario_id,
                        principalTable: "maquinarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_servico_maquinarios_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_avaliado_id",
                table: "avaliacoes",
                column: "avaliado_id");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_avaliador_id",
                table: "avaliacoes",
                column: "avaliador_id");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_servico_id_avaliador_id_avaliado_id",
                table: "avaliacoes",
                columns: new[] { "servico_id", "avaliador_id", "avaliado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_servico_id_usuario_id",
                table: "candidaturas",
                columns: new[] { "servico_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidaturas_usuario_id",
                table: "candidaturas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_denuncias_denunciado_id",
                table: "denuncias",
                column: "denunciado_id");

            migrationBuilder.CreateIndex(
                name: "IX_denuncias_denunciante_id",
                table: "denuncias",
                column: "denunciante_id");

            migrationBuilder.CreateIndex(
                name: "IX_maquinarios_nome",
                table: "maquinarios",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portfolios_usuario_id",
                table: "portfolios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_servico_maquinarios_maquinario_id",
                table: "servico_maquinarios",
                column: "maquinario_id");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_usuario_id",
                table: "servicos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_maquinarios_maquinario_id",
                table: "usuario_maquinarios",
                column: "maquinario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_nome_usuario",
                table: "usuarios",
                column: "nome_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_verificacoes_usuario_id",
                table: "verificacoes",
                column: "usuario_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacoes");

            migrationBuilder.DropTable(
                name: "candidaturas");

            migrationBuilder.DropTable(
                name: "denuncias");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropTable(
                name: "servico_maquinarios");

            migrationBuilder.DropTable(
                name: "usuario_maquinarios");

            migrationBuilder.DropTable(
                name: "verificacoes");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "maquinarios");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
