# Fioo Backend - Documentação do Projeto

Este documento fornece uma visão geral da arquitetura, autenticação e rotas da API do projeto Fioo.

## 1. Visão Geral
O Fioo é um sistema de intermediação de serviços, conectando fornecedores a clientes. O backend é construído em .NET (ASP.NET Core Web API) utilizando Entity Framework Core para persistência de dados.

## 2. Arquitetura
O projeto segue o padrão clássico de Web API em camadas:
- **Controllers**: Localizados em `/Controller`, gerenciam as requisições HTTP e as respostas.
- **DTOs**: Localizados em `/Controller/DTOs`, definem os objetos de transferência de dados para entrada e saída.
- **Entities**: Localizados em `/Entities`, representam os modelos de domínio e as tabelas do banco de dados.
- **Data**: Localizado em `/Data`, contém o `AppDbContext` e as configurações das entidades (Fluent API) em `/Data/Configurations`.
- **Enums**: Localizados em `/Enums`, definem os estados e tipos utilizados no sistema.
- **Utils**: Localizados em `/Utils`, contém classes utilitárias para validação e lógica comum.

### Tecnologias
- **Framework**: .NET 8+ (ASP.NET Core)
- **Banco de Dados**: PostgreSQL (via Npgsql.EntityFrameworkCore.PostgreSQL)
- **Documentação**: Swagger / OpenAPI
- **ORM**: Entity Framework Core

## 3. Autenticação e Autorização
A autenticação é baseada em **JWT (JSON Web Token)**.

- **Processo de Login**: `POST /api/usuarios/login`
- **Registro**: `POST /api/usuarios`
- **Claims do Token**:
  - `sub`: ID do usuário
  - `email`: Email do usuário
  - `nome`: Nome completo
  - `nome_usuario`: Username
  - `tipo`: Tipo do usuário (Cliente, Fornecedor, etc.)

A autorização é aplicada via atributo `[Authorize]` em controllers ou métodos específicos. Algumas rotas possuem validações adicionais baseadas no `UsuarioTipo` contido no token.

## 4. Rotas da API

### Usuários (`/api/usuarios`)
- `POST /`: Cadastra um novo usuário (senha com PBKDF2 + salt).
- `POST /login`: Autentica e retorna um token JWT. Hashes antigos (SHA-256) são migrados para PBKDF2 no login.
- `POST /verificar-email`: Verifica se um email já está em uso.
- `GET /costureiros` e `GET /fornecedores` (Requer `[Authorize]`): tela Encontrar. Query: `busca`, `uf`, `cidade`, `avaliacaoMin` (1–5), `ordenacao` (`relevantes`, `avaliacao-alta`, `avaliacao-baixa`, `az`, `za`), `pagina`, `tamanhoPagina`. Retorna `{ itens, pagina, temMais }` com média e total de avaliações no papel.
- `GET /{id}` (Requer `[Authorize]`): dados completos só do próprio usuário; para os demais, os dados públicos.
- `GET /{id}/publico` (Requer `[Authorize]`): perfil público (sem e-mail, documento ou telefone).
- `DELETE /{id}` (Requer `[Authorize]`): exclui a própria conta; 409 se houver histórico (candidaturas, avaliações, denúncias).
- `GET /me` / `PUT /me` (Requer `[Authorize]`): dados e atualização do usuário autenticado.

O campo `SenhaHash` nunca é serializado nas respostas.

### Serviços (`/api/servicos`) - Requer `[Authorize]`
- `GET /`: Lista serviços de outros fornecedores. Query: `busca`, `uf`, `cidade`, `valorMin`, `valorMax`, `cobranca`, `prazo`, `categoria`, `status`, `ordenacao` (`relevantes`, `prazo-proximo`, `prazo-distante`, `maior-valor`, `menor-valor`), `pagina`, `tamanhoPagina`. Retorna `{ itens, pagina, temMais }`.
- `GET /categorias`: Categorias já cadastradas, sem repetir variações de maiúsculas/acentos.
- `GET /meus/{usuarioId}`: Lista os serviços criados pelo usuário especificado.
- `GET /{id}`: Obtém detalhes de um serviço (inclui `costureiroVinculado`).
- `POST /`: Cria um serviço (apenas `Fornecedor`). Nasce "Em andamento". Data do prazo só em "Data Específica".
- `PUT /{id}`: Atualiza um serviço (apenas o proprietário; não altera o status).
- `PATCH /{id}/status`: Em andamento → Concluído (exige costureiro vinculado) ou Cancelado (apenas o proprietário).
- `GET /{id}/candidaturas`: Candidatos do serviço (apenas o proprietário).
- `POST /{id}/candidaturas/{candidaturaId}/aceitar`: Aceita um candidato e recusa os demais pendentes.
- `DELETE /{id}`: Remove um serviço (apenas o proprietário; 409 se já avaliado).

### Candidaturas (`/api/candidaturas`) - Requer `[Authorize]`
- `GET /`: Lista todas as candidaturas.
- `GET /{id}`: Obtém detalhes de uma candidatura.
- `GET /servico/{servicoId}`: Lista candidaturas para um serviço específico.
- `GET /em-andamento/{usuarioId}`: Candidaturas do próprio usuário (403 para outro usuário).
- `POST /`: Candidata o usuário do token a um serviço em andamento e sem costureiro vinculado.
- `PUT /{id}/status`: Pendente → Recusada (dono do serviço) ou Cancelada (candidato). Para aceitar, use o endpoint de serviços.
- `DELETE /{id}`: Remove uma candidatura não aceita (candidato ou dono do serviço).

### Avaliações (`/api/avaliacoes`) - Requer `[Authorize]`
- `POST /`: Avalia a outra parte de um serviço Concluído (nota geral, comunicação e qualidade de 1 a 5; comentário opcional até 300 caracteres). Uma por participante.
- `GET /feitas`: Ids dos serviços já avaliados pelo usuário do token.
- `GET /servico/{servicoId}`: Avaliações de um serviço.
- `GET /usuario/{usuarioId}?papel=`: Avaliações recebidas, com média e total como costureiro e como fornecedor.

### Erros
Respostas de erro usam `{ field?, message }` em pt-BR (400, 403, 404, 409, 422).

### Denúncias (`/api/denuncias`)
- `GET /`: Lista todas as denúncias.
- `GET /{id}`: Obtém detalhes de uma denúncia.
- `GET /denunciante/{usuarioId}`: Lista denúncias feitas por um usuário.
- `GET /denunciado/{usuarioId}`: Lista denúncias recebidas por um usuário.
- `POST /`: Cria uma nova denúncia.
- `PUT /{id}/status`: Atualiza o status de uma denúncia.
- `DELETE /{id}`: Remove uma denúncia.

### Maquinários (`/api/maquinarios`)
- `GET /`: Lista todos os maquinários cadastrados no sistema.
- `GET /{id}`: Obtém detalhes de um maquinário.
- `POST /`: Cadastra um novo maquinário.
- `DELETE /{id}`: Remove um maquinário.

### Portfólios (`/api/portfolios`)
- `GET /`: Lista todos os registros de portfólio.
- `GET /{id}`: Obtém um registro específico.
- `GET /usuario/{usuarioId}`: Lista o portfólio de um usuário.
- `POST /`: Adiciona um item ao portfólio.
- `PUT /{id}`: Atualiza um item do portfólio.
- `DELETE /{id}`: Remove um item do portfólio.

## 5. Convenções de Código
- **Nomenclatura**: PascalCase para classes, métodos e propriedades. camelCase para variáveis locais e parâmetros.
- **Configurações**: Uso de `Fluent API` em `/Data/Configurations` para mapeamento de banco de dados.
- **Validações**: Lógica de validação concentrada em `Utils/ValidationHelpers.cs`.

## 6. Fluxo de Desenvolvimento
- **Migrations**: O projeto utiliza Migrations do EF Core. Para aplicar novas alterações no banco:
  ```bash
  dotnet ef migrations add <NomeDaMigration>
  dotnet ef database update
  ```
- **Uploads**: Arquivos de mídia (fotos de perfil e portfólio) são salvos em `wwwroot/uploads`.
