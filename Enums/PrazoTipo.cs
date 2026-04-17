using System.ComponentModel;
namespace Fioo.Enums;

public enum PrazoTipo
{
    [Description("Semanal")]
    Semanal = 0,
    [Description("Quinzenal")]
    Quinzenal = 1,
    [Description("Mensal")]
    Mensal = 2,
    [Description("DataEspecifica")]
    DataEspecifica = 3
}