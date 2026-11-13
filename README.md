# Fioo Backend

API backend em ASP.NET Core e Entity Framework Core para a plataforma Fioo.

- **Swagger**: `http://localhost:5131/swagger/index.html` (ou porta configurada)

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 10 SDK
- PostgreSQL

### Envio de e-mails (SMTP Gmail)
O Fioo envia e-mails como **Fioo <fioo.sistema@gmail.com>** por `smtp.gmail.com:587` (STARTTLS). As configurações não secretas estão na seção `EmailSettings` do `appsettings.json`.

A senha de aplicativo do Google é configurada **somente** pela variável de ambiente:
```bash
EmailSettings__SmtpPassword=sua-senha-de-16-caracteres
```
(O nome antigo `Email__SenhaApp` / `Email:SenhaApp` em User Secrets continua aceito.)

Os links dos e-mails usam `FrontendUrl` (em produção, variável de ambiente `FrontendUrl`).

Para testar o envio real sem passar pela recuperação de senha (não sobe a API nem expõe endpoint):
```bash
EmailSettings__SmtpPassword=... dotnet run -- --testar-email destinatario@exemplo.com
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
- `POST /api/usuarios/esqueci-senha`: Envia e-mail (template HTML do Fioo) com link contendo o token de uso único (expira em 30 min). Resposta genérica para evitar enumeração de usuários; no máximo um envio por minuto por conta.
- `POST /api/usuarios/redefinir-senha`: Valida o hash do token e altera a senha do usuário.

### Upload de Fotos
- `POST /api/usuarios/{id}/foto` (também acessível por `/costureiros/{id}/foto` e `/fornecedores/{id}/foto`):
  - Formato: `multipart/form-data` (`foto`)
  - Formatos aceitos: `.jpg`, `.jpeg`, `.png`, `.webp`
  - Tamanho máximo: 2 MB
  - Requer autenticação (o usuário logado deve ser o proprietário do ID).