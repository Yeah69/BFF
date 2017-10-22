using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper.Extensions;

namespace BFF.MVVM.ValidationRules
{
    class NoEmptyFileNameRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string possibleFilePath = (string) value;
            bool validate = !string.IsNullOrEmpty(new FileInfo(possibleFilePath).Name);
            return new ValidationResult(validate, validate ? null : "ValidationRule_NotExistingSavePath_EmptyName".Localize<string>());
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
