using System.Globalization;
using System.Windows.Controls;

namespace BFF.WPFStuff.ValidationRules
{
    class NoNullRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = value != null;
            return new ValidationResult(validate, validate ? null : "This property is not valid!"); //todo: Localize
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
