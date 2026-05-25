using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fioo.Migrations
{
    /// <inheritdoc />
    public partial class RenameToPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_avaliacoes_servicos_servico_id",
                table: "avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_avaliacoes_usuarios_avaliado_id",
                table: "avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_avaliacoes_usuarios_avaliador_id",
                table: "avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_candidaturas_servicos_servico_id",
                table: "candidaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_candidaturas_usuarios_usuario_id",
                table: "candidaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_denuncias_usuarios_denunciado_id",
                table: "denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_denuncias_usuarios_denunciante_id",
                table: "denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_portfolios_usuarios_usuario_id",
                table: "portfolios");

            migrationBuilder.DropForeignKey(
                name: "FK_servico_maquinarios_maquinarios_maquinario_id",
                table: "servico_maquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_servico_maquinarios_servicos_servico_id",
                table: "servico_maquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_servicos_usuarios_usuario_id",
                table: "servicos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_maquinarios_maquinarios_maquinario_id",
                table: "usuario_maquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_maquinarios_usuarios_usuario_id",
                table: "usuario_maquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_verificacoes_usuarios_usuario_id",
                table: "verificacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_verificacoes",
                table: "verificacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_servicos",
                table: "servicos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portfolios",
                table: "portfolios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_maquinarios",
                table: "maquinarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_denuncias",
                table: "denuncias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_candidaturas",
                table: "candidaturas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_avaliacoes",
                table: "avaliacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuario_maquinarios",
                table: "usuario_maquinarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_servico_maquinarios",
                table: "servico_maquinarios");

            migrationBuilder.RenameTable(
                name: "verificacoes",
                newName: "Verificacoes");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "servicos",
                newName: "Servicos");

            migrationBuilder.RenameTable(
                name: "portfolios",
                newName: "Portfolios");

            migrationBuilder.RenameTable(
                name: "maquinarios",
                newName: "Maquinarios");

            migrationBuilder.RenameTable(
                name: "denuncias",
                newName: "Denuncias");

            migrationBuilder.RenameTable(
                name: "candidaturas",
                newName: "Candidaturas");

            migrationBuilder.RenameTable(
                name: "avaliacoes",
                newName: "Avaliacoes");

            migrationBuilder.RenameTable(
                name: "usuario_maquinarios",
                newName: "UsuarioMaquinarios");

            migrationBuilder.RenameTable(
                name: "servico_maquinarios",
                newName: "ServicoMaquinarios");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Verificacoes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "Verificacoes",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "foto_selfie_url",
                table: "Verificacoes",
                newName: "FotoSelfieUrl");

            migrationBuilder.RenameColumn(
                name: "etapa_tempo",
                table: "Verificacoes",
                newName: "EtapaTempo");

            migrationBuilder.RenameColumn(
                name: "etapa_email",
                table: "Verificacoes",
                newName: "EtapaEmail");

            migrationBuilder.RenameColumn(
                name: "etapa_documento",
                table: "Verificacoes",
                newName: "EtapaDocumento");

            migrationBuilder.RenameColumn(
                name: "data_verificacao_completa",
                table: "Verificacoes",
                newName: "DataVerificacaoCompleta");

            migrationBuilder.RenameColumn(
                name: "data_envio_documento",
                table: "Verificacoes",
                newName: "DataEnvioDocumento");

            migrationBuilder.RenameIndex(
                name: "IX_verificacoes_usuario_id",
                table: "Verificacoes",
                newName: "IX_Verificacoes_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "Usuarios",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "telefone",
                table: "Usuarios",
                newName: "Telefone");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "Usuarios",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Usuarios",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "cidade",
                table: "Usuarios",
                newName: "Cidade");

            migrationBuilder.RenameColumn(
                name: "ativo",
                table: "Usuarios",
                newName: "Ativo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Usuarios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "telefone_visivel",
                table: "Usuarios",
                newName: "TelefoneVisivel");

            migrationBuilder.RenameColumn(
                name: "senha_hash",
                table: "Usuarios",
                newName: "SenhaHash");

            migrationBuilder.RenameColumn(
                name: "nome_usuario",
                table: "Usuarios",
                newName: "NomeUsuario");

            migrationBuilder.RenameColumn(
                name: "foto_perfil_url",
                table: "Usuarios",
                newName: "FotoPerfilUrl");

            migrationBuilder.RenameColumn(
                name: "data_cadastro",
                table: "Usuarios",
                newName: "DataCadastro");

            migrationBuilder.RenameColumn(
                name: "cpf_cnpj",
                table: "Usuarios",
                newName: "CpfCnpj");

            migrationBuilder.RenameColumn(
                name: "anos_experiencia",
                table: "Usuarios",
                newName: "AnosExperiencia");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_email",
                table: "Usuarios",
                newName: "IX_Usuarios_Email");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_nome_usuario",
                table: "Usuarios",
                newName: "IX_Usuarios_NomeUsuario");

            migrationBuilder.RenameColumn(
                name: "valor",
                table: "Servicos",
                newName: "Valor");

            migrationBuilder.RenameColumn(
                name: "titulo",
                table: "Servicos",
                newName: "Titulo");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Servicos",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "Servicos",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "Servicos",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "cidade",
                table: "Servicos",
                newName: "Cidade");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Servicos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "Servicos",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "tipo_prazo",
                table: "Servicos",
                newName: "TipoPrazo");

            migrationBuilder.RenameColumn(
                name: "tipo_cobranca",
                table: "Servicos",
                newName: "TipoCobranca");

            migrationBuilder.RenameColumn(
                name: "data_prazo",
                table: "Servicos",
                newName: "DataPrazo");

            migrationBuilder.RenameColumn(
                name: "data_criacao",
                table: "Servicos",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "categoria_servico",
                table: "Servicos",
                newName: "CategoriaServico");

            migrationBuilder.RenameIndex(
                name: "IX_servicos_usuario_id",
                table: "Servicos",
                newName: "IX_Servicos_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "Portfolios",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Portfolios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "Portfolios",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "foto_url",
                table: "Portfolios",
                newName: "FotoUrl");

            migrationBuilder.RenameColumn(
                name: "data_upload",
                table: "Portfolios",
                newName: "DataUpload");

            migrationBuilder.RenameIndex(
                name: "IX_portfolios_usuario_id",
                table: "Portfolios",
                newName: "IX_Portfolios_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Maquinarios",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Maquinarios",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_maquinarios_nome",
                table: "Maquinarios",
                newName: "IX_Maquinarios_Nome");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Denuncias",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "motivo",
                table: "Denuncias",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "Denuncias",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Denuncias",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "denunciante_id",
                table: "Denuncias",
                newName: "DenuncianteId");

            migrationBuilder.RenameColumn(
                name: "denunciado_id",
                table: "Denuncias",
                newName: "DenunciadoId");

            migrationBuilder.RenameColumn(
                name: "data_denuncia",
                table: "Denuncias",
                newName: "DataDenuncia");

            migrationBuilder.RenameIndex(
                name: "IX_denuncias_denunciante_id",
                table: "Denuncias",
                newName: "IX_Denuncias_DenuncianteId");

            migrationBuilder.RenameIndex(
                name: "IX_denuncias_denunciado_id",
                table: "Denuncias",
                newName: "IX_Denuncias_DenunciadoId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Candidaturas",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Candidaturas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "Candidaturas",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "servico_id",
                table: "Candidaturas",
                newName: "ServicoId");

            migrationBuilder.RenameColumn(
                name: "data_candidatura",
                table: "Candidaturas",
                newName: "DataCandidatura");

            migrationBuilder.RenameColumn(
                name: "data_atualizacao",
                table: "Candidaturas",
                newName: "DataAtualizacao");

            migrationBuilder.RenameIndex(
                name: "IX_candidaturas_usuario_id",
                table: "Candidaturas",
                newName: "IX_Candidaturas_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_candidaturas_servico_id_usuario_id",
                table: "Candidaturas",
                newName: "IX_Candidaturas_ServicoId_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "nota",
                table: "Avaliacoes",
                newName: "Nota");

            migrationBuilder.RenameColumn(
                name: "comentario",
                table: "Avaliacoes",
                newName: "Comentario");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Avaliacoes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "servico_id",
                table: "Avaliacoes",
                newName: "ServicoId");

            migrationBuilder.RenameColumn(
                name: "data_avaliacao",
                table: "Avaliacoes",
                newName: "DataAvaliacao");

            migrationBuilder.RenameColumn(
                name: "avaliador_id",
                table: "Avaliacoes",
                newName: "AvaliadorId");

            migrationBuilder.RenameColumn(
                name: "avaliado_id",
                table: "Avaliacoes",
                newName: "AvaliadoId");

            migrationBuilder.RenameIndex(
                name: "IX_avaliacoes_servico_id_avaliador_id_avaliado_id",
                table: "Avaliacoes",
                newName: "IX_Avaliacoes_ServicoId_AvaliadorId_AvaliadoId");

            migrationBuilder.RenameIndex(
                name: "IX_avaliacoes_avaliador_id",
                table: "Avaliacoes",
                newName: "IX_Avaliacoes_AvaliadorId");

            migrationBuilder.RenameIndex(
                name: "IX_avaliacoes_avaliado_id",
                table: "Avaliacoes",
                newName: "IX_Avaliacoes_AvaliadoId");

            migrationBuilder.RenameColumn(
                name: "maquinario_id",
                table: "UsuarioMaquinarios",
                newName: "MaquinarioId");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "UsuarioMaquinarios",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_maquinarios_maquinario_id",
                table: "UsuarioMaquinarios",
                newName: "IX_UsuarioMaquinarios_MaquinarioId");

            migrationBuilder.RenameColumn(
                name: "maquinario_id",
                table: "ServicoMaquinarios",
                newName: "MaquinarioId");

            migrationBuilder.RenameColumn(
                name: "servico_id",
                table: "ServicoMaquinarios",
                newName: "ServicoId");

            migrationBuilder.RenameIndex(
                name: "IX_servico_maquinarios_maquinario_id",
                table: "ServicoMaquinarios",
                newName: "IX_ServicoMaquinarios_MaquinarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Verificacoes",
                table: "Verificacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servicos",
                table: "Servicos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Portfolios",
                table: "Portfolios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maquinarios",
                table: "Maquinarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Denuncias",
                table: "Denuncias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Candidaturas",
                table: "Candidaturas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Avaliacoes",
                table: "Avaliacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuarioMaquinarios",
                table: "UsuarioMaquinarios",
                columns: new[] { "UsuarioId", "MaquinarioId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicoMaquinarios",
                table: "ServicoMaquinarios",
                columns: new[] { "ServicoId", "MaquinarioId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Usuarios_AvaliadoId",
                table: "Avaliacoes",
                column: "AvaliadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Usuarios_AvaliadorId",
                table: "Avaliacoes",
                column: "AvaliadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidaturas_Servicos_ServicoId",
                table: "Candidaturas",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidaturas_Usuarios_UsuarioId",
                table: "Candidaturas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Usuarios_DenunciadoId",
                table: "Denuncias",
                column: "DenunciadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Usuarios_DenuncianteId",
                table: "Denuncias",
                column: "DenuncianteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolios_Usuarios_UsuarioId",
                table: "Portfolios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicoMaquinarios_Maquinarios_MaquinarioId",
                table: "ServicoMaquinarios",
                column: "MaquinarioId",
                principalTable: "Maquinarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicoMaquinarios_Servicos_ServicoId",
                table: "ServicoMaquinarios",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Servicos_Usuarios_UsuarioId",
                table: "Servicos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioMaquinarios_Maquinarios_MaquinarioId",
                table: "UsuarioMaquinarios",
                column: "MaquinarioId",
                principalTable: "Maquinarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioMaquinarios_Usuarios_UsuarioId",
                table: "UsuarioMaquinarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Verificacoes_Usuarios_UsuarioId",
                table: "Verificacoes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Servicos_ServicoId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Usuarios_AvaliadoId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Usuarios_AvaliadorId",
                table: "Avaliacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidaturas_Servicos_ServicoId",
                table: "Candidaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidaturas_Usuarios_UsuarioId",
                table: "Candidaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Usuarios_DenunciadoId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Usuarios_DenuncianteId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Portfolios_Usuarios_UsuarioId",
                table: "Portfolios");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicoMaquinarios_Maquinarios_MaquinarioId",
                table: "ServicoMaquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicoMaquinarios_Servicos_ServicoId",
                table: "ServicoMaquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Servicos_Usuarios_UsuarioId",
                table: "Servicos");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioMaquinarios_Maquinarios_MaquinarioId",
                table: "UsuarioMaquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioMaquinarios_Usuarios_UsuarioId",
                table: "UsuarioMaquinarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Verificacoes_Usuarios_UsuarioId",
                table: "Verificacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Verificacoes",
                table: "Verificacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servicos",
                table: "Servicos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Portfolios",
                table: "Portfolios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maquinarios",
                table: "Maquinarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Denuncias",
                table: "Denuncias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Candidaturas",
                table: "Candidaturas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Avaliacoes",
                table: "Avaliacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioMaquinarios",
                table: "UsuarioMaquinarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicoMaquinarios",
                table: "ServicoMaquinarios");

            migrationBuilder.RenameTable(
                name: "Verificacoes",
                newName: "verificacoes");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuarios");

            migrationBuilder.RenameTable(
                name: "Servicos",
                newName: "servicos");

            migrationBuilder.RenameTable(
                name: "Portfolios",
                newName: "portfolios");

            migrationBuilder.RenameTable(
                name: "Maquinarios",
                newName: "maquinarios");

            migrationBuilder.RenameTable(
                name: "Denuncias",
                newName: "denuncias");

            migrationBuilder.RenameTable(
                name: "Candidaturas",
                newName: "candidaturas");

            migrationBuilder.RenameTable(
                name: "Avaliacoes",
                newName: "avaliacoes");

            migrationBuilder.RenameTable(
                name: "UsuarioMaquinarios",
                newName: "usuario_maquinarios");

            migrationBuilder.RenameTable(
                name: "ServicoMaquinarios",
                newName: "servico_maquinarios");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "verificacoes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "verificacoes",
                newName: "usuario_id");

            migrationBuilder.RenameColumn(
                name: "FotoSelfieUrl",
                table: "verificacoes",
                newName: "foto_selfie_url");

            migrationBuilder.RenameColumn(
                name: "EtapaTempo",
                table: "verificacoes",
                newName: "etapa_tempo");

            migrationBuilder.RenameColumn(
                name: "EtapaEmail",
                table: "verificacoes",
                newName: "etapa_email");

            migrationBuilder.RenameColumn(
                name: "EtapaDocumento",
                table: "verificacoes",
                newName: "etapa_documento");

            migrationBuilder.RenameColumn(
                name: "DataVerificacaoCompleta",
                table: "verificacoes",
                newName: "data_verificacao_completa");

            migrationBuilder.RenameColumn(
                name: "DataEnvioDocumento",
                table: "verificacoes",
                newName: "data_envio_documento");

            migrationBuilder.RenameIndex(
                name: "IX_Verificacoes_UsuarioId",
                table: "verificacoes",
                newName: "IX_verificacoes_usuario_id");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "usuarios",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "usuarios",
                newName: "telefone");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "usuarios",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "usuarios",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Cidade",
                table: "usuarios",
                newName: "cidade");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "usuarios",
                newName: "ativo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usuarios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TelefoneVisivel",
                table: "usuarios",
                newName: "telefone_visivel");

            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "usuarios",
                newName: "senha_hash");

            migrationBuilder.RenameColumn(
                name: "NomeUsuario",
                table: "usuarios",
                newName: "nome_usuario");

            migrationBuilder.RenameColumn(
                name: "FotoPerfilUrl",
                table: "usuarios",
                newName: "foto_perfil_url");

            migrationBuilder.RenameColumn(
                name: "DataCadastro",
                table: "usuarios",
                newName: "data_cadastro");

            migrationBuilder.RenameColumn(
                name: "CpfCnpj",
                table: "usuarios",
                newName: "cpf_cnpj");

            migrationBuilder.RenameColumn(
                name: "AnosExperiencia",
                table: "usuarios",
                newName: "anos_experiencia");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_Email",
                table: "usuarios",
                newName: "IX_usuarios_email");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_NomeUsuario",
                table: "usuarios",
                newName: "IX_usuarios_nome_usuario");

            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "servicos",
                newName: "valor");

            migrationBuilder.RenameColumn(
                name: "Titulo",
                table: "servicos",
                newName: "titulo");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "servicos",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "servicos",
                newName: "estado");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "servicos",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "Cidade",
                table: "servicos",
                newName: "cidade");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "servicos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "servicos",
                newName: "usuario_id");

            migrationBuilder.RenameColumn(
                name: "TipoPrazo",
                table: "servicos",
                newName: "tipo_prazo");

            migrationBuilder.RenameColumn(
                name: "TipoCobranca",
                table: "servicos",
                newName: "tipo_cobranca");

            migrationBuilder.RenameColumn(
                name: "DataPrazo",
                table: "servicos",
                newName: "data_prazo");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "servicos",
                newName: "data_criacao");

            migrationBuilder.RenameColumn(
                name: "CategoriaServico",
                table: "servicos",
                newName: "categoria_servico");

            migrationBuilder.RenameIndex(
                name: "IX_Servicos_UsuarioId",
                table: "servicos",
                newName: "IX_servicos_usuario_id");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "portfolios",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "portfolios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "portfolios",
                newName: "usuario_id");

            migrationBuilder.RenameColumn(
                name: "FotoUrl",
                table: "portfolios",
                newName: "foto_url");

            migrationBuilder.RenameColumn(
                name: "DataUpload",
                table: "portfolios",
                newName: "data_upload");

            migrationBuilder.RenameIndex(
                name: "IX_Portfolios_UsuarioId",
                table: "portfolios",
                newName: "IX_portfolios_usuario_id");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "maquinarios",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "maquinarios",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarios_Nome",
                table: "maquinarios",
                newName: "IX_maquinarios_nome");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "denuncias",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "denuncias",
                newName: "motivo");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "denuncias",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "denuncias",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DenuncianteId",
                table: "denuncias",
                newName: "denunciante_id");

            migrationBuilder.RenameColumn(
                name: "DenunciadoId",
                table: "denuncias",
                newName: "denunciado_id");

            migrationBuilder.RenameColumn(
                name: "DataDenuncia",
                table: "denuncias",
                newName: "data_denuncia");

            migrationBuilder.RenameIndex(
                name: "IX_Denuncias_DenuncianteId",
                table: "denuncias",
                newName: "IX_denuncias_denunciante_id");

            migrationBuilder.RenameIndex(
                name: "IX_Denuncias_DenunciadoId",
                table: "denuncias",
                newName: "IX_denuncias_denunciado_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "candidaturas",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "candidaturas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "candidaturas",
                newName: "usuario_id");

            migrationBuilder.RenameColumn(
                name: "ServicoId",
                table: "candidaturas",
                newName: "servico_id");

            migrationBuilder.RenameColumn(
                name: "DataCandidatura",
                table: "candidaturas",
                newName: "data_candidatura");

            migrationBuilder.RenameColumn(
                name: "DataAtualizacao",
                table: "candidaturas",
                newName: "data_atualizacao");

            migrationBuilder.RenameIndex(
                name: "IX_Candidaturas_UsuarioId",
                table: "candidaturas",
                newName: "IX_candidaturas_usuario_id");

            migrationBuilder.RenameIndex(
                name: "IX_Candidaturas_ServicoId_UsuarioId",
                table: "candidaturas",
                newName: "IX_candidaturas_servico_id_usuario_id");

            migrationBuilder.RenameColumn(
                name: "Nota",
                table: "avaliacoes",
                newName: "nota");

            migrationBuilder.RenameColumn(
                name: "Comentario",
                table: "avaliacoes",
                newName: "comentario");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "avaliacoes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServicoId",
                table: "avaliacoes",
                newName: "servico_id");

            migrationBuilder.RenameColumn(
                name: "DataAvaliacao",
                table: "avaliacoes",
                newName: "data_avaliacao");

            migrationBuilder.RenameColumn(
                name: "AvaliadorId",
                table: "avaliacoes",
                newName: "avaliador_id");

            migrationBuilder.RenameColumn(
                name: "AvaliadoId",
                table: "avaliacoes",
                newName: "avaliado_id");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacoes_ServicoId_AvaliadorId_AvaliadoId",
                table: "avaliacoes",
                newName: "IX_avaliacoes_servico_id_avaliador_id_avaliado_id");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacoes_AvaliadorId",
                table: "avaliacoes",
                newName: "IX_avaliacoes_avaliador_id");

            migrationBuilder.RenameIndex(
                name: "IX_Avaliacoes_AvaliadoId",
                table: "avaliacoes",
                newName: "IX_avaliacoes_avaliado_id");

            migrationBuilder.RenameColumn(
                name: "MaquinarioId",
                table: "usuario_maquinarios",
                newName: "maquinario_id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "usuario_maquinarios",
                newName: "usuario_id");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioMaquinarios_MaquinarioId",
                table: "usuario_maquinarios",
                newName: "IX_usuario_maquinarios_maquinario_id");

            migrationBuilder.RenameColumn(
                name: "MaquinarioId",
                table: "servico_maquinarios",
                newName: "maquinario_id");

            migrationBuilder.RenameColumn(
                name: "ServicoId",
                table: "servico_maquinarios",
                newName: "servico_id");

            migrationBuilder.RenameIndex(
                name: "IX_ServicoMaquinarios_MaquinarioId",
                table: "servico_maquinarios",
                newName: "IX_servico_maquinarios_maquinario_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_verificacoes",
                table: "verificacoes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_servicos",
                table: "servicos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portfolios",
                table: "portfolios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_maquinarios",
                table: "maquinarios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_denuncias",
                table: "denuncias",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_candidaturas",
                table: "candidaturas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_avaliacoes",
                table: "avaliacoes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuario_maquinarios",
                table: "usuario_maquinarios",
                columns: new[] { "usuario_id", "maquinario_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_servico_maquinarios",
                table: "servico_maquinarios",
                columns: new[] { "servico_id", "maquinario_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_avaliacoes_servicos_servico_id",
                table: "avaliacoes",
                column: "servico_id",
                principalTable: "servicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_avaliacoes_usuarios_avaliado_id",
                table: "avaliacoes",
                column: "avaliado_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_avaliacoes_usuarios_avaliador_id",
                table: "avaliacoes",
                column: "avaliador_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_candidaturas_servicos_servico_id",
                table: "candidaturas",
                column: "servico_id",
                principalTable: "servicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_candidaturas_usuarios_usuario_id",
                table: "candidaturas",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_denuncias_usuarios_denunciado_id",
                table: "denuncias",
                column: "denunciado_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_denuncias_usuarios_denunciante_id",
                table: "denuncias",
                column: "denunciante_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_portfolios_usuarios_usuario_id",
                table: "portfolios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_servico_maquinarios_maquinarios_maquinario_id",
                table: "servico_maquinarios",
                column: "maquinario_id",
                principalTable: "maquinarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_servico_maquinarios_servicos_servico_id",
                table: "servico_maquinarios",
                column: "servico_id",
                principalTable: "servicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_servicos_usuarios_usuario_id",
                table: "servicos",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_maquinarios_maquinarios_maquinario_id",
                table: "usuario_maquinarios",
                column: "maquinario_id",
                principalTable: "maquinarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_maquinarios_usuarios_usuario_id",
                table: "usuario_maquinarios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_verificacoes_usuarios_usuario_id",
                table: "verificacoes",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
