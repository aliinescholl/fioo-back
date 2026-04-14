namespace Fioo.Entities;
using Fioo.Enums;
using System.Collections.Generic;

public class Usuario : EntidadeBase
{
    public string Nome { get; set; }
    public string NomeUsuario { get; set; }
    public string Email { get; set; }
    public string SenhaHash { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public bool TelefoneVisivel { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public UsuarioTipo Tipo { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public int? AnosExperiencia { get; set; }

    public Verificacao? Verificacao { get; set; }
    public List<Servico>? Servicos { get; set; }
    public List<Candidatura>? Candidaturas { get; set; }
    public List<Portfolio>? Portfolios { get; set; }
    public List<Avaliacao>? AvaliacoesFeitas { get; set; }
    public List<Avaliacao>? AvaliacoesRecebidas { get; set; }
    public List<Denuncia>? DenunciasFeitas { get; set; }
    public List<UsuarioMaquinario>? Maquinarios { get; set; }
}