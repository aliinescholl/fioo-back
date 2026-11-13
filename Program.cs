using Fioo.Data;
using Fioo.Services;
using Fioo.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// wwwroot precisa existir na inicialização; sem isso os uploads (fotos) não são servidos
var webRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRoot);
builder.Environment.WebRootPath = webRoot;
builder.Environment.WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot);

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de leitura da requisição (ex.: ?valorMin=abc, JSON malformado) em pt-BR,
        // no mesmo formato { field, message } usado pelos controllers
        options.InvalidModelStateResponseFactory = context =>
        {
            var erro = context.ModelState.FirstOrDefault(e => e.Value?.Errors.Count > 0);
            var campo = erro.Key?.Split('.', '[').LastOrDefault(p => p != "$" && p != "") ?? "";
            if (campo.Length > 0)
                campo = char.ToLowerInvariant(campo[0]) + campo[1..];
            var mensagem = campo.Length > 0
                ? $"O campo \"{campo}\" está com um valor inválido."
                : "Os dados enviados estão incompletos ou em formato inválido.";
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { field = campo, message = mensagem });
        };
    });
// OpenAPI / Swagger (se tiver Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Serviço de consulta CNPJ via API pública cnpj.ws
builder.Services.AddHttpClient<CnpjWsService>();

// E-mail (SMTP Gmail). A senha de aplicativo vem só da variável de ambiente EmailSettings__SmtpPassword.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.Secao));
builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection(FrontendSettings.Secao));
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // ajustar se usar issuer/audience
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
}

var app = builder.Build();

// Teste manual do SMTP: dotnet run -- --testar-email destinatario@exemplo.com (envia e encerra, sem subir a API)
if (EmailTesteCli.FoiSolicitado(args))
{
    Environment.ExitCode = await EmailTesteCli.ExecutarAsync(app.Services, args);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Servir arquivos estáticos (imagens de perfil/portfólio)
app.UseStaticFiles();

if (!string.IsNullOrEmpty(jwtKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

app.Run();

// Exposto para os testes de integração (WebApplicationFactory<Program>)
public partial class Program { }
