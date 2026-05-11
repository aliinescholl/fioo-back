using Fioo.Enums;

namespace Fioo.DTOs;

public class UsuarioResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } 
    public string NomeUsuario { get; set; } 
    public string? FotoPerfilUrl { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
}

public class MaquinarioResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } 
}

public class ServicoResumoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } 
    public string? Descricao { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? CategoriaServico { get; set; }
    public decimal? Valor { get; set; }
    public CobrancaTipo TipoCobranca { get; set; }
    public PrazoTipo? TipoPrazo { get; set; }
    public DateOnly? DataPrazo { get; set; }
    public ServicoStatus Status { get; set; }
    public DateTime DataCriacao { get; set; }

    public UsuarioResumoDto Usuario { get; set; } 
    public List<MaquinarioResumoDto> Maquinarios { get; set; } = [];
}

public class CandidaturaDto
{
    public int Id { get; set; }
    public CandidaturaStatus Status { get; set; }
    public DateTime DataCandidatura { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public ServicoResumoDto Servico { get; set; } 
}

public class CriarCandidaturaDto
{
    public int UsuarioId { get; set; }
    public int ServicoId { get; set; }
}