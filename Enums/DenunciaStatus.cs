using System.ComponentModel;
namespace Fioo.Enums;

public enum DenunciaStatus
{
    [Description("Aberta")]
    Aberta = 0,
    [Description("EmAnalise")]
    EmAnalise = 1,
    [Description("Resolvida")]
    Resolvida = 2,
    [Description("Arquivada")]
    Arquivada = 3
}