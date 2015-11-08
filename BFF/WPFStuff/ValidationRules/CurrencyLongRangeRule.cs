using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BFF.WPFStuff.ValidationRules
{
    class CurrencyLongRangeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal outVar;
            bool parsed = decimal.TryParse((string)value, NumberStyles.Currency,
              cultureInfo.NumberFormat, out outVar);
            if (parsed)
            {
                decimal factor = (decimal)Math.Pow(10, cultureInfo.NumberFormat.CurrencyDecimalDigits);
                outVar = outVar * factor;
                if (outVar > long.MaxValue || outVar < long.MinValue)
                    return new ValidationResult(false, $"Value is out of range: [ {(long.MinValue/factor).ToString("C", cultureInfo.NumberFormat)} .. {(long.MaxValue / factor).ToString("C", cultureInfo.NumberFormat)} ]");

            }
            return new ValidationResult(true, null); 
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
