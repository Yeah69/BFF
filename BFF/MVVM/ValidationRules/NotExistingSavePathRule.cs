using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper.Extensions;

namespace BFF.MVVM.ValidationRules
{
    class NotExistingSavePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string possibleFilePath = (string) value;
            bool validate = possibleFilePath != null && Directory.Exists(new FileInfo(possibleFilePath).DirectoryName);
            return new ValidationResult(validate, validate ? null : "ValidationRule_NotExistingSavePath".Localize<string>());
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
