# Fioo Backend

API backend em ASP.NET Core e Entity Framework Core para a plataforma Fioo.

- **Swagger**: `http://localhost:5131/swagger/index.html` (ou porta configurada)

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 10 SDK
- PostgreSQL

### Configuração de Secrets (Recuperação de Senha)
Para envio de e-mails em desenvolvimento local, configure a senha de app do Gmail via User Secrets:

```bash
dotnet user-secrets set "Email:SenhaApp" "sua-senha-de-16-caracteres"
```

No ambiente de hospedagem/produção, configure como variável de ambiente:
```bash
Email__SenhaApp=sua-senha-de-16-caracteres
```

> **Atenção:** Nunca versione nem envie a senha de aplicativo para o repositório.

### Migrations do Banco de Dados
Para aplicar as migrações no PostgreSQL:
```bash
dotnet ef database update
```

### Rodar a aplicação
```bash
dotnet run
```

### Rodar os Testes
```bash
dotnet test
```

---

## 📌 Principais Endpoints Novos

### Recuperação de Senha
- `POST /api/usuarios/esqueci-senha`: Envia e-mail com link contendo o token de uso único (expira em 30 min). Resposta genérica para evitar enumeração de usuários.
- `POST /api/usuarios/redefinir-senha`: Valida o hash do token e altera a senha do usuário.

### Upload de Fotos
- `POST /api/usuarios/{id}/foto` (também acessível por `/costureiros/{id}/foto` e `/fornecedores/{id}/foto`):
  - Formato: `multipart/form-data` (`foto`)
  - Formatos aceitos: `.jpg`, `.jpeg`, `.png`, `.webp`
  - Tamanho máximo: 2 MB
  - Requer autenticação (o usuário logado deve ser o proprietário do ID).