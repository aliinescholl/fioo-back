using System.ComponentModel;
namespace Fioo.Enums;

public enum CobrancaTipo
{
    [Description("PorPeca")]
    PorPeca = 0,
    [Description("PorOperacao")]
    PorOperacao = 1
}