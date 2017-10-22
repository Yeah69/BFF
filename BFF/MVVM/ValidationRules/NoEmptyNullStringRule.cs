using System.Globalization;
using System.Windows.Controls;
using BFF.Helper.Extensions;

namespace BFF.MVVM.ValidationRules
{
    class NoEmptyNullStringRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = !string.IsNullOrEmpty((string) value);
            return new ValidationResult(validate, validate ? null : "ValidationRule_NoNull".Localize<string>());
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
