using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BFF.WPFStuff.ValidationRules
{
    class NoEmptyNullStringRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = !string.IsNullOrEmpty((string) value);
            return new ValidationResult(validate, validate ? null : "The text may not be empty!"); 
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
