using System;
using System.Globalization;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class CurrencyLongRangeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal outVar;
            CultureInfo currencyCulture = BffEnvironment.CultureProvider.CurrencyCulture;
            decimal factor = (decimal)Math.Pow(10, currencyCulture.NumberFormat.CurrencyDecimalDigits);
            string message = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_CurrencyLongRange", null, BffEnvironment.CultureProvider.LanguageCulture);
            message = string.Format(message, (long.MinValue/factor).ToString("C", currencyCulture.NumberFormat), (long.MaxValue/factor).ToString("C", currencyCulture.NumberFormat));
            bool parsed = decimal.TryParse((string)value, NumberStyles.Currency,
              currencyCulture.NumberFormat, out outVar);
            if (parsed)
            {
                outVar = outVar * factor;
                if (outVar > long.MaxValue || outVar < long.MinValue)
                    return new ValidationResult(false, message);
                return new ValidationResult(true, null);

            }
            return new ValidationResult(false, message);
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
