using System.Globalization;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class CurrencyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal outVar;
            bool validate = decimal.TryParse((string) value, NumberStyles.Currency,
                BffEnvironment.CultureProvider.CurrencyCulture.NumberFormat, out outVar);
            return new ValidationResult(validate, validate ? null : "The Currency format could not be parsed!"); 
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
