using System.Security.Cryptography;
using System.Text;

namespace Fioo.Utils;

/// <summary>
/// Hash de senha com PBKDF2-SHA256 e salt aleatório, no formato "pbkdf2$iteracoes$salt$hash".
/// Hashes antigos (SHA-256 sem salt, em Base64) continuam aceitos no login e devem ser
/// substituídos pelo formato novo assim que o usuário entrar (PrecisaAtualizar).
/// </summary>
public static class SenhaHasher
{
    private const string Prefixo = "pbkdf2";
    private const int Iteracoes = 210_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public static string Gerar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return $"{Prefixo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string? hashArmazenado)
    {
        if (string.IsNullOrEmpty(hashArmazenado))
            return false;

        var partes = hashArmazenado.Split('$');
        if (partes.Length == 4 && partes[0] == Prefixo && int.TryParse(partes[1], out var iteracoes))
        {
            try
            {
                var salt = Convert.FromBase64String(partes[2]);
                var esperado = Convert.FromBase64String(partes[3]);
                var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
                return CryptographicOperations.FixedTimeEquals(calculado, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Formato antigo: SHA-256 sem salt
        var legado = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(senha)));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(legado), Encoding.UTF8.GetBytes(hashArmazenado));
    }

    /// <summary>True se o hash está no formato antigo ou com menos iterações que o atual.</summary>
    public static bool PrecisaAtualizar(string hashArmazenado)
    {
        var partes = hashArmazenado.Split('$');
        return partes.Length != 4 || partes[0] != Prefixo || !int.TryParse(partes[1], out var it) || it < Iteracoes;
    }
}
