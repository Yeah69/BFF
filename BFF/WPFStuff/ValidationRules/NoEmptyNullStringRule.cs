using System.Globalization;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class NoEmptyNullStringRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = !string.IsNullOrEmpty((string) value);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NoNull", null, BffEnvironment.CultureProvider.LanguageCulture));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
