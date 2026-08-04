using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fioo.Services;

public class CnpjWsService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CnpjWsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://publica.cnpj.ws/");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Consulta o CNPJ na API pública do cnpj.ws.
    /// </summary>
    /// <param name="cnpj">CNPJ sem máscara (apenas dígitos)</param>
    /// <returns>Dados do CNPJ ou null se não encontrado</returns>
    /// <exception cref="CnpjWsException">Lançada quando a API retorna erro</exception>
    public async Task<CnpjWsResponse?> ConsultarAsync(string cnpj)
    {
        var response = await _httpClient.GetAsync($"cnpj/{cnpj}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new CnpjWsException(CnpjWsErro.NaoEncontrado, "CNPJ não encontrado na base de dados.");

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new CnpjWsException(CnpjWsErro.RateLimitAtingido, "Limite de consultas atingido. Tente novamente em 1 minuto.");

        if (!response.IsSuccessStatusCode)
            throw new CnpjWsException(CnpjWsErro.ErroGenerico, $"Erro ao consultar CNPJ: {(int)response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CnpjWsResponse>(json, _jsonOptions);
    }
}

// ─── Modelo de resposta ──────────────────────────────────────────────────────

public class CnpjWsResponse
{
    [JsonPropertyName("cnpj_raiz")]
    public string? CnpjRaiz { get; set; }

    [JsonPropertyName("razao_social")]
    public string? RazaoSocial { get; set; }

    [JsonPropertyName("capital_social")]
    public string? CapitalSocial { get; set; }

    [JsonPropertyName("estabelecimento")]
    public CnpjWsEstabelecimento? Estabelecimento { get; set; }
}

public class CnpjWsEstabelecimento
{
    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome_fantasia")]
    public string? NomeFantasia { get; set; }

    [JsonPropertyName("situacao_cadastral")]
    public string? SituacaoCadastral { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("ddd1")]
    public string? Ddd1 { get; set; }

    [JsonPropertyName("telefone1")]
    public string? Telefone1 { get; set; }

    [JsonPropertyName("cidade")]
    public CnpjWsCidade? Cidade { get; set; }

    [JsonPropertyName("estado")]
    public CnpjWsEstado? Estado { get; set; }
}

public class CnpjWsCidade
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }
}

public class CnpjWsEstado
{
    [JsonPropertyName("sigla")]
    public string? Sigla { get; set; }
}

// ─── Exceção específica ──────────────────────────────────────────────────────

public enum CnpjWsErro
{
    NaoEncontrado,
    RateLimitAtingido,
    ErroGenerico
}

public class CnpjWsException : Exception
{
    public CnpjWsErro Erro { get; }

    public CnpjWsException(CnpjWsErro erro, string message) : base(message)
    {
        Erro = erro;
    }
}
