using System.ComponentModel;
namespace Fioo.Enums;

public enum ServicoStatus
{
    [Description("Ativo")]
    Ativo = 0,
    [Description("EmAndamento")]
    EmAndamento = 1,
    [Description("Finalizado")]
    Finalizado = 2,
    [Description("Cancelado")]
    Cancelado = 3
}