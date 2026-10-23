using Fioo.Enums;

namespace Fioo.Utils;

public static class PrazoHelper
{
    private static readonly TimeZoneInfo FusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Data usada para ordenar serviços por prazo.
    /// Data Específica: a própria data. Semanal/Quinzenal/Mensal: data de criação
    /// (no fuso America/Sao_Paulo) + 7/15/30 dias. Sem tipo de prazo: null.
    /// </summary>
    public static DateOnly? CalcularDataReferencia(PrazoTipo? tipoPrazo, DateOnly? dataPrazo, DateTime dataCriacaoUtc)
    {
        var dataCriacaoLocal = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dataCriacaoUtc, DateTimeKind.Utc), FusoBrasilia));

        return tipoPrazo switch
        {
            PrazoTipo.DataEspecifica => dataPrazo,
            PrazoTipo.Semanal => dataCriacaoLocal.AddDays(7),
            PrazoTipo.Quinzenal => dataCriacaoLocal.AddDays(15),
            PrazoTipo.Mensal => dataCriacaoLocal.AddDays(30),
            _ => null
        };
    }
}
