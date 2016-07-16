﻿using System.Globalization;
using System.Windows.Controls;
using BFF.Properties;

namespace BFF.WPFStuff.ValidationRules
{
    class CurrencyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal outVar;
            bool validate = decimal.TryParse((string) value, NumberStyles.Currency,
                Settings.Default.Culture_SessionCurrency.NumberFormat, out outVar);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_Currency", null, Settings.Default.Culture_DefaultLanguage));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
