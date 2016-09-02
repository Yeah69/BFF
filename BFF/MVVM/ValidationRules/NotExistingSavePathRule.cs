using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Properties;

namespace BFF.MVVM.ValidationRules
{
    class NotExistingSavePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string possibleFilePath = (string) value;
            bool validate = Directory.Exists(new FileInfo(possibleFilePath).DirectoryName);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NotExistingSavePath", null, Settings.Default.Culture_DefaultLanguage));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
