﻿using System.Globalization;
using System.Windows.Controls;
using BFF.Properties;

namespace BFF.WPFStuff.ValidationRules
{
    class NoNullRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = value != null;
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NoNull", null, Settings.Default.Culture_DefaultLanguage));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
