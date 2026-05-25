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
- `POST /`: Cadastra um novo usuário.
- `POST /login`: Autentica um usuário e retorna um token JWT.
- `POST /verificar-email`: Verifica se um email já está em uso.
- `GET /`: Lista todos os usuários.
- `GET /{id}`: Obtém detalhes de um usuário específico.
- `DELETE /{id}`: Remove um usuário.
- `GET /me`: Obtém os dados do usuário autenticado (Requer `[Authorize]`).
- `PUT /me`: Atualiza o perfil do usuário autenticado, incluindo upload de foto e portfólio (Requer `[Authorize]`).

### Serviços (`/api/servicos`) - Requer `[Authorize]`
- `GET /`: Lista todos os serviços ativos (excluindo os do próprio usuário).
- `GET /meus/{usuarioId}`: Lista os serviços criados pelo usuário especificado.
- `GET /{id}`: Obtém detalhes de um serviço.
- `GET /usuario/{usuarioId}`: Lista serviços de um usuário.
- `POST /`: Cria um novo serviço (Apenas para `UsuarioTipo.Fornecedor`).
- `PUT /{id}`: Atualiza um serviço existente (Apenas o proprietário).
- `DELETE /{id}`: Remove um serviço (Apenas o proprietário).

### Candidaturas (`/api/candidaturas`)
- `GET /`: Lista todas as candidaturas.
- `GET /{id}`: Obtém detalhes de uma candidatura.
- `GET /servico/{servicoId}`: Lista candidaturas para um serviço específico.
- `GET /em-andamento/{usuarioId}`: Lista candidaturas em andamento para um usuário.
- `POST /`: Cria uma nova candidatura para um serviço.
- `PUT /{id}/status`: Atualiza o status de uma candidatura.
- `DELETE /{id}`: Remove uma candidatura.

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
