namespace Fioo.Entities;
using Fioo.Enums;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string NomeUsuario { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SenhaHash { get; set; } = null!;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public bool TelefoneVisivel { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public UsuarioTipo Tipo { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public int? AnosExperiencia { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    public Verificacao? Verificacao { get; set; }
    public ICollection<Servico> Servicos { get; set; } = [];
    public ICollection<Candidatura> Candidaturas { get; set; } = [];
    public ICollection<Portfolio> Portfolios { get; set; } = [];
    public ICollection<Avaliacao> AvaliacoesFeitas { get; set; } = [];
    public ICollection<Avaliacao> AvaliacoesRecebidas { get; set; } = [];
    public ICollection<Denuncia> DenunciasFeitas { get; set; } = [];
    public ICollection<UsuarioMaquinario> Maquinarios { get; set; } = [];
}