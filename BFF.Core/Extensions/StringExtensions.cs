using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace BFF.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex MatchAllIllegalFilePathCharacters = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]");

        public static string RemoveIllegalFilePathCharacters(this string @this)
        {
            return MatchAllIllegalFilePathCharacters.Replace(@this, "");
        }

        public static long CurrencyAsLong(this string value, CultureInfo culture)
        {
            bool isConverted = decimal.TryParse(value, NumberStyles.Currency,
                culture.NumberFormat, out var decimalValue);
            if (!isConverted)
                throw new ValidationException();
            return (long)(decimalValue * (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits));
        }
    }
}
