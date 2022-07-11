using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace InscricaoChll.Api.Extensions;

public static class StringExtensions
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        if (email.Contains(";"))
        {
            var results = new List<bool>();
            foreach (var e in email.Split(";"))
            {
                results.Add(e.IsValidEmail());
            }

            return results.All(result => result);
        }

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static bool IsCpf(this string numero)
    {
        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        string tempCpf;
        string digito;
        int soma;
        int resto;
        numero = numero.Trim();
        numero = numero.Replace(".", "").Replace("-", "");
        if (numero.Length != 11)
            return false;
        tempCpf = numero.Substring(0, 9);
        soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;
        digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;
        digito = digito + resto.ToString();
        return numero.EndsWith(digito);
    }

    public static bool IsCnpj(this string numero)
    {
        int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma;
        int resto;
        string digito;
        string tempCnpj;
        numero = numero.Trim();
        numero = numero.Replace(".", "").Replace("-", "").Replace("/", "");
        if (numero.Length != 14)
            return false;
        tempCnpj = numero.Substring(0, 12);
        soma = 0;
        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;
        digito = resto.ToString();
        tempCnpj = tempCnpj + digito;
        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;
        digito = digito + resto.ToString();
        return numero.EndsWith(digito);
    }

    public static bool IsUrl(this string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri);
    }

    public static string Base64Encode(this string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(this string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static bool IsBase64Encoded(this string base64EncodedData)
    {
        try
        {
            var test = Base64Decode(base64EncodedData);
            return true;
        }
        catch (Exception)
        {
        }

        return false;
    }

    public static string HideCardNumber(this string numbers)
    {
        numbers = numbers.RemoveNonNumeric();
        var sb = new StringBuilder();
        sb.Append(numbers.Substring(0, 6));
        sb.Append(new String('*', (numbers.Length - 10)));
        sb.Append(numbers.Substring(numbers.Length - 4));
        return sb.ToString();
    }

    public static string RemoveNonNumeric(this string numbers)
    {
        return !string.IsNullOrEmpty(numbers)
            ? Regex.Replace(numbers, @"\D", "")
            : String.Empty;
    }

    public static string GenerateTokenString(this string text, int length = 6)
    {
        return new string(Enumerable
            .Repeat(Guid.NewGuid().ToString().Replace("-", String.Empty), length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }

    public static string RemoveDiacritics(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    public static string ToCurrencyString(this decimal ammount, string language = "pt-BR")
    {
        return ammount.ToString("C", new CultureInfo(language));
    }
}