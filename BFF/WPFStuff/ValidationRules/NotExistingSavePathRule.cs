using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class NotExistingSavePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string possibleFilePath = (string) value;
            bool validate = Directory.Exists(new FileInfo(possibleFilePath).DirectoryName);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NotExistingSavePath", null, BffEnvironment.CultureProvider.LanguageCulture));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
