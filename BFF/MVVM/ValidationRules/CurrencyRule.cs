using System.Globalization;
using System.Windows.Controls;
using BFF.Properties;

namespace BFF.MVVM.ValidationRules
{
    class CurrencyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = decimal.TryParse((string) value, NumberStyles.Currency,
                Settings.Default.Culture_SessionCurrency.NumberFormat, out decimal outVar);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_Currency", null, Settings.Default.Culture_DefaultLanguage));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
