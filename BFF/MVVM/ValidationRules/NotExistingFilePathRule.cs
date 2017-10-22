using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper.Extensions;

namespace BFF.MVVM.ValidationRules
{
    class NotExistingFilePathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = File.Exists((string)value);
            return new ValidationResult(validate, validate ? null : "ValidationRule_NotExistingFilePath".Localize<string>());
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
