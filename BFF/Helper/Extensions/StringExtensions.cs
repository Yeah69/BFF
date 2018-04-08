using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using BFF.Properties;
using WPFLocalizeExtension.Extensions;

namespace BFF.Helper.Extensions
{
    internal static class StringExtensions
    {
        public static T Localize<T>(this string key) =>
            LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);

        public static string Localize(this string key) =>
            LocExtension.GetLocalizedValue<string>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);

        public static long CurrencyAsLong(this string value)
        {
            return value.CurrencyAsLong(Settings.Default.Culture_SessionCurrency ?? CultureInfo.CurrentCulture);
        }

        public static long CurrencyAsLong(this string value, CultureInfo culture)
        {
            bool isConverted = Decimal.TryParse(value, NumberStyles.Currency,
                culture.NumberFormat, out var decimalValue);
            if (!isConverted)
                throw new ValidationException();
            return (long)(decimalValue * (decimal)Math.Pow(10, culture.NumberFormat.CurrencyDecimalDigits));
        }

        public static bool IsNullOrWhiteSpace(this string @this) => string.IsNullOrWhiteSpace(@this);

        public static bool IsNullOrEmpty(this string @this) => string.IsNullOrEmpty(@this);
    }
}
