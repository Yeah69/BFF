using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using BFF.Properties;

namespace BFF.Helper
{
    static class Output
    {
        public static CultureInfo CurrencyCulture = null;

        public static void WriteLine(string text, [CallerMemberName] string callerName = null) //todo: Replace with NLog
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write(@"{0}:", callerName);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(@" {0}", text);
        }

        public static string AsCurrency(this long value)
        {
            return value.AsCurrency(Settings.Default.Culture_SessionCurrency ?? CultureInfo.CurrentCulture);
        }

        public static string AsCurrency(this long value, CultureInfo culture)
        {
            decimal result = value / (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits);
            return result.ToString("C", culture);
        }

        public static long CurrencyAsLong(this string value)
        {
            return value.CurrencyAsLong(Settings.Default.Culture_SessionCurrency ?? CultureInfo.CurrentCulture);
        }

        public static long CurrencyAsLong(this string value, CultureInfo culture)
        {
            decimal decimalValue;
            bool isConverted = Decimal.TryParse(value, NumberStyles.Currency,
              culture.NumberFormat, out decimalValue);
            if(!isConverted)
                throw new ValidationException();
            return (long) (decimalValue*(decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits));
        }
    }
}
