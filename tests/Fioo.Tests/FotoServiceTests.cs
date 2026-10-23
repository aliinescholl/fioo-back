using Fioo.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using Xunit;

namespace Fioo.Tests;

public class FotoServiceTests : IDisposable
{
    private readonly TestWebHostEnvironment _env;
    private readonly FotoService _service;
    private readonly string _testDir;

    public FotoServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "fioo-foto-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        _env = new TestWebHostEnvironment { WebRootPath = _testDir };
        _service = new FotoService(_env);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    private static IFormFile CriarArquivoFake(string nome, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "foto", nome)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Theory]
    [InlineData("perfil.jpg")]
    [InlineData("perfil.jpeg")]
    [InlineData("perfil.png")]
    [InlineData("perfil.webp")]
    public async Task FotoService_AceitaExtensoesValidas(string nomeArquivo)
    {
        var arquivo = CriarArquivoFake(nomeArquivo, Encoding.UTF8.GetBytes("fake-image-content"));
        var url = await _service.SalvarFotoPerfilAsync(arquivo);

        Assert.NotNull(url);
        Assert.StartsWith("/uploads/avatars/", url);
        Assert.DoesNotContain(nomeArquivo, url); // Nome próprio com GUID, sem expor nome enviado
    }

    [Theory]
    [InlineData("documento.pdf")]
    [InlineData("script.exe")]
    [InlineData("animacao.gif")]
    public async Task FotoService_RejeitaExtensoesInvalidas(string nomeArquivo)
    {
        var arquivo = CriarArquivoFake(nomeArquivo, Encoding.UTF8.GetBytes("fake-content"));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.SalvarFotoPerfilAsync(arquivo));
        Assert.Contains("Formato de foto inválido", ex.Message);
    }

    [Fact]
    public async Task FotoService_RejeitaArquivoMaiorQue2MB()
    {
        var bytes = new byte[2 * 1024 * 1024 + 1]; // 2MB + 1 byte
        var arquivo = CriarArquivoFake("imagem_gigante.jpg", bytes);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.SalvarFotoPerfilAsync(arquivo));
        Assert.Contains("excede o tamanho máximo", ex.Message);
    }

    [Fact]
    public async Task FotoService_ApagaFotoAntigaAoTrocar()
    {
        var arquivo1 = CriarArquivoFake("foto1.jpg", Encoding.UTF8.GetBytes("img1"));
        var urlAntiga = await _service.SalvarFotoPerfilAsync(arquivo1);

        var nomeArquivoAntigo = Path.GetFileName(urlAntiga);
        var caminhoFisicoAntigo = Path.Combine(_testDir, "uploads", "avatars", nomeArquivoAntigo);
        Assert.True(File.Exists(caminhoFisicoAntigo));

        var arquivo2 = CriarArquivoFake("foto2.png", Encoding.UTF8.GetBytes("img2"));
        var urlNova = await _service.SalvarFotoPerfilAsync(arquivo2, urlAntiga);

        // Foto antiga deve ter sido removida do disco
        Assert.False(File.Exists(caminhoFisicoAntigo));
        // Nova foto deve existir
        var caminhoFisicoNovo = Path.Combine(_testDir, "uploads", "avatars", Path.GetFileName(urlNova));
        Assert.True(File.Exists(caminhoFisicoNovo));
    }
}
