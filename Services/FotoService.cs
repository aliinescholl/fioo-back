using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Fioo.Services;

public class FotoService
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] PermittedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB

    public FotoService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SalvarFotoPerfilAsync(IFormFile arquivo, string? fotoAntigaUrl = null)
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ArgumentException("Nenhum arquivo de imagem foi enviado.");

        if (arquivo.Length > MaxFileSize)
            throw new ArgumentException("Arquivo da foto excede o tamanho máximo permitido (2 MB).");

        var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !PermittedExtensions.Contains(ext))
            throw new ArgumentException("Formato de foto inválido. Permitidos: .jpg, .jpeg, .png e .webp.");

        var uploadsDir = ObterDiretorioUploads();
        Directory.CreateDirectory(uploadsDir);

        // Apagar foto antiga se for um arquivo local em /uploads/avatars/
        ApagarFotoAntiga(fotoAntigaUrl, uploadsDir);

        var novoNome = $"{Guid.NewGuid()}{ext}";
        var caminhoCompleto = Path.Combine(uploadsDir, novoNome);

        using (var stream = File.Create(caminhoCompleto))
        {
            await arquivo.CopyToAsync(stream);
        }

        return $"/uploads/avatars/{novoNome}";
    }

    public void ApagarFotoAntiga(string? fotoUrl, string? uploadsDir = null)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl)) return;

        try
        {
            var dir = uploadsDir ?? ObterDiretorioUploads();
            var nomeArquivo = Path.GetFileName(fotoUrl);
            var caminho = Path.Combine(dir, nomeArquivo);

            if (File.Exists(caminho))
            {
                File.Delete(caminho);
            }
        }
        catch
        {
            // Ignora falha de exclusão para não interromper fluxo
        }
    }

    private string ObterDiretorioUploads()
    {
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        return Path.Combine(webRoot, "uploads", "avatars");
    }
}
