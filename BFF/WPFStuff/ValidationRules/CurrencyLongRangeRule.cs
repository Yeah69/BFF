﻿using System;
using System.Globalization;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.ValidationRules
{
    class CurrencyLongRangeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            decimal outVar;
            decimal factor = (decimal)Math.Pow(10, cultureInfo.NumberFormat.CurrencyDecimalDigits);
            string message = $"Value is out of range: [ {(long.MinValue/factor).ToString("C", cultureInfo.NumberFormat)} .. {(long.MaxValue/factor).ToString("C", cultureInfo.NumberFormat)} ]"; //Output.CurrencyCulture.NumberFormat)} ]";
            bool parsed = decimal.TryParse((string)value, NumberStyles.Currency,
              cultureInfo.NumberFormat, out outVar);
            if (parsed)
            {
                outVar = outVar * factor;
                if (outVar > long.MaxValue || outVar < long.MinValue)
                    return new ValidationResult(false, message);
                return new ValidationResult(true, null);

            }
            return new ValidationResult(false, message);
            // The "Invalid"-Message is only relevant if validate is false
        }
    }
}
