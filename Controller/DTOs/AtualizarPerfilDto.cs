using Microsoft.AspNetCore.Http;
using Fioo.Enums;
using System.Collections.Generic;

public class EnderecoDto
{
    public string? Rua { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
}

public class AtualizarPerfilDto
{
    public UsuarioTipo? ServicoPrestado { get; set; } // Costureiro ou Fornecedor
    public string? Nome { get; set; }
    public string? NomeSocial { get; set; }
    public string? Pronome { get; set; }
    public string? NomeUsuario { get; set; }
    public string? Email { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public EnderecoDto? Endereco { get; set; }
    public int? AnosExperiencia { get; set; } // Tempo de serviço na área
    public List<int>? MaquinarioIds { get; set; } // Lista de IDs de maquinário
    public bool? TelefoneVisivel { get; set; } // Mostrar número para outros usuários
    // Uploads via form-data
    public IFormFile? FotoPerfil { get; set; }
    public List<IFormFile>? Portfolios { get; set; } // Fotos do portfólio (placeholder)
    // Campo de verificação visual (não executa lógica por enquanto)
    public bool? VerificarPerfil { get; set; }
}