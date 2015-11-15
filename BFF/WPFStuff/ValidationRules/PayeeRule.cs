using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using BFF.DB.SQLite;

namespace BFF.WPFStuff.ValidationRules
{
    class PayeeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = SqLiteHelper.AllPayees.Contains(value);
            return new ValidationResult(validate, validate ? null : "The selected Payee does not exist!"); 
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
