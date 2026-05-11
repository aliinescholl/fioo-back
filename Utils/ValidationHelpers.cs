using System.Text.RegularExpressions;

namespace Fioo.Utils;

public static class ValidationHelpers
{
    // Email simples (válido para a maioria dos casos)
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool ValidarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email.Trim());
    }

    // Validação simples de CPF (algoritmo oficial)
    public static bool ValidarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var numbers = Regex.Replace(cpf, @"[^\d]", "");
        if (numbers.Length != 11) return false;
        if (new string(numbers[0], numbers.Length) == numbers) return false;

        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var temp = numbers.Substring(0, 9);
        int sum = 0;
        for (int i = 0; i < 9; i++) sum += int.Parse(temp[i].ToString()) * mult1[i];

        int remainder = sum % 11;
        int digit = remainder < 2 ? 0 : 11 - remainder;
        temp += digit;
        sum = 0;
        for (int i = 0; i < 10; i++) sum += int.Parse(temp[i].ToString()) * mult2[i];

        remainder = sum % 11;
        digit = remainder < 2 ? 0 : 11 - remainder;
        return numbers.EndsWith(digit.ToString());
    }

    // Validação simples de CNPJ (algoritmo)
    public static bool ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return false;
        var numbers = Regex.Replace(cnpj, @"[^\d]", "");
        if (numbers.Length != 14) return false;
        if (new string(numbers[0], numbers.Length) == numbers) return false;

        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var temp = numbers.Substring(0, 12);
        int sum = 0;
        for (int i = 0; i < 12; i++) sum += int.Parse(temp[i].ToString()) * mult1[i];

        int remainder = sum % 11;
        int digit = remainder < 2 ? 0 : 11 - remainder;
        temp += digit;
        sum = 0;
        for (int i = 0; i < 13; i++) sum += int.Parse(temp[i].ToString()) * mult2[i];

        remainder = sum % 11;
        digit = remainder < 2 ? 0 : 11 - remainder;
        return numbers.EndsWith(digit.ToString());
    }

    public static bool ValidarCpfOuCnpj(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var digits = Regex.Replace(value, @"[^\d]", "");
        if (digits.Length == 11) return ValidarCpf(digits);
        if (digits.Length == 14) return ValidarCnpj(digits);
        return false;
    }

    // Senha forte: pelo menos 8 caracteres, 1 maiúscula, 1 minúscula e 1 caractere especial
    public static bool ValidarSenhaForte(string senha)
    {
        if (string.IsNullOrEmpty(senha) || senha.Length < 8) return false;
        bool temMaiuscula = false, temMinuscula = false, temEspecial = false;
        foreach (var c in senha)
        {
            if (char.IsUpper(c)) temMaiuscula = true;
            else if (char.IsLower(c)) temMinuscula = true;
            else if (!char.IsLetterOrDigit(c)) temEspecial = true;
        }
        return temMaiuscula && temMinuscula && temEspecial;
    }
}