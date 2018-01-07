using System;
using System.Globalization;
using BFF.Properties;

namespace BFF.Helper.Extensions
{
    internal static class LongExtensions
    {

        public static string AsCurrency(this long value)
        {
            return value.AsCurrency(Settings.Default.Culture_SessionCurrency ?? CultureInfo.CurrentCulture);
        }

        public static string AsCurrency(this long value, CultureInfo culture)
        {
            decimal result = value / (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits);
            return result.ToString("C", culture);
        }
    }
}
