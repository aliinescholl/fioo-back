using System.ComponentModel;
namespace Fioo.Enums;

/// <summary>
/// Status do serviço. Nasce em "Em andamento" (publicado e ainda não encerrado);
/// "Concluído" e "Cancelado" são estados finais. Ver ServicoStatusRegras.
/// </summary>
public enum ServicoStatus
{
    [Description("Em andamento")]
    EmAndamento = 1,
    [Description("Concluído")]
    Concluido = 2,
    [Description("Cancelado")]
    Cancelado = 3
}
