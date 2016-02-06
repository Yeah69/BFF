using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace BFF.Helper
{
    static class Output
    {
        public static CultureInfo CurrencyCulture = null;

        public static void WriteLine(string text, [CallerMemberName] string callerName = null)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write(@"{0}:", callerName);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(@" {0}", text);
        }

        public static string AsCurrency(this long value)
        {
            return value.AsCurrency(BffEnvironment.CultureProvider.CurrencyCulture ?? CultureInfo.CurrentCulture);
        }

        public static string AsCurrency(this long value, CultureInfo culture)
        {
            decimal result = value / (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits);
            return result.ToString("C", culture);
        }

        public static long CurrencyAsLong(this string value)
        {
            return value.CurrencyAsLong(BffEnvironment.CultureProvider.CurrencyCulture ?? CultureInfo.CurrentCulture);
        }

        public static long CurrencyAsLong(this string value, CultureInfo culture)
        {
            decimal decval;
            bool convt = Decimal.TryParse(value, NumberStyles.Currency,
              culture.NumberFormat, out decval);
            if(!convt)
                throw new ValidationException();
            return (long) (decval*(decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits));
        }
    }
}
