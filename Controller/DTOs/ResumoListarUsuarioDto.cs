namespace Fioo.Controller.DTOs
{
    /// <summary>Card de usuário na tela Encontrar.</summary>
    public class ResumoListarUsuarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string NomeUsuario { get; set; } = null!;
        public string? Localizacao { get; set; }
        public string? Foto { get; set; }
        // Média da nota geral no papel listado (costureiro ou fornecedor); null se não há avaliações
        public double? Media { get; set; }
        public int TotalAvaliacoes { get; set; }
    }

    /// <summary>Filtros e ordenação da tela Encontrar (query string).</summary>
    public class UsuarioFiltroDto
    {
        public string? Busca { get; set; }
        public string? Uf { get; set; }
        public string? Cidade { get; set; }
        // Mínimo de estrelas (1 a 5): exclui quem não tem avaliação
        public int? AvaliacaoMin { get; set; }

        /// <summary>relevantes (padrão), avaliacao-alta, avaliacao-baixa, az, za</summary>
        public string? Ordenacao { get; set; }

        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class PaginaUsuariosDto
    {
        public List<ResumoListarUsuarioDto> Itens { get; set; } = [];
        public int Pagina { get; set; }
        public bool TemMais { get; set; }
    }

    /// <summary>Dados públicos de um usuário (sem e-mail, documento ou telefone).</summary>
    public class PerfilPublicoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string NomeUsuario { get; set; } = null!;
        public string? FotoPerfilUrl { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public Fioo.Enums.UsuarioTipo Tipo { get; set; }
        public int? AnosExperiencia { get; set; }
    }
}
