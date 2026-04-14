using System.ComponentModel;

namespace Fioo.Enums;

public enum CandidaturaStatus
{
    [Description("Pendente")]
    Pendente = 0,
    [Description("Aceita")]
    Aceita = 1,
    [Description("Recusada")]
    Recusada = 2,
    [Description("Cancelada")]
    Cancelada = 3
}