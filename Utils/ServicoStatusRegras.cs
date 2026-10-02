using Fioo.Enums;

namespace Fioo.Utils;

public static class ServicoStatusRegras
{
    /// <summary>
    /// Transições permitidas: Em andamento → Concluído e Em andamento → Cancelado.
    /// Concluído e Cancelado são estados finais.
    /// </summary>
    public static bool TransicaoPermitida(ServicoStatus atual, ServicoStatus novo) =>
        atual == ServicoStatus.EmAndamento
        && (novo == ServicoStatus.Concluido || novo == ServicoStatus.Cancelado);
}
