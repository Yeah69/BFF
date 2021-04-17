using System;
using System.Globalization;

namespace BFF.Core.Extensions
{
    public static class LongExtensions
    {
        public static string AsCurrency(this long value, CultureInfo culture)
        {
            decimal result = value / (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits);
            return result.ToString("C", culture);
        }
    }
}
