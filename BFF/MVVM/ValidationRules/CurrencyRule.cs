using System.Globalization;
using System.Windows.Controls;
using BFF.Helper.Extensions;
using BFF.Properties;

namespace BFF.MVVM.ValidationRules
{
    class CurrencyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = decimal.TryParse((string) value, NumberStyles.Currency,
                Settings.Default.Culture_SessionCurrency.NumberFormat, out decimal _);
            return new ValidationResult(validate, validate ? null : "ValidationRule_Currency".Localize<string>());
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
