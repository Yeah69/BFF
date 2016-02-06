using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class NotExistingFilePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = File.Exists((string)value);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NotExistingFilePath", null, BffEnvironment.CultureProvider.LanguageCulture));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
