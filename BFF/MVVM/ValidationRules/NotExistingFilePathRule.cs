using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Properties;

namespace BFF.MVVM.ValidationRules
{
    class NotExistingFilePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = File.Exists((string)value);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NotExistingFilePath", null, Settings.Default.Culture_DefaultLanguage));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
