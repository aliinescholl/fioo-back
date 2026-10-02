namespace Fioo.Controller.DTOs
{
    public class ResumoListarUsuarioDto
    {
        public string Nome { get; set; }
        public string? Localizacao { get; set; }
        public string? Foto { get; set; }
        public string MediaEstrela { get; set; }
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
