using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class NoEmptyFileNameRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string possibleFilePath = (string) value;
            bool validate = !string.IsNullOrEmpty(new FileInfo(possibleFilePath).Name);
            return new ValidationResult(validate, validate ? null : (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("ValidationRule_NotExistingSavePath_EmptyName", null, BffEnvironment.CultureProvider.LanguageCulture));
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
