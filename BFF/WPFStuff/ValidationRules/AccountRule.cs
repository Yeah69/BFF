using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BFF.DB.SQLite;

namespace BFF.WPFStuff.ValidationRules
{
    class AccountRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool validate = SqLiteHelper.AllAccounts.Contains(value);
            return new ValidationResult(validate, validate ? null : "The selected Account does not exist!"); 
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
