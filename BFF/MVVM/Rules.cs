using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using BFF.Helper.Extensions;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;

namespace BFF.MVVM
{
    internal static class Rules
    {
        public static ValidationRule EmptyPayee =
            LambdaConverters.Validator.Create<IPayeeViewModel>(
                e => e.Value != null
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "ErrorMessageEmptyPayee".Localize()));

        public static ValidationRule EmptyCategory =
            LambdaConverters.Validator.Create<ICategoryBaseViewModel>(
                e => e.Value != null
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "ErrorMessageEmptyCategory".Localize()));

        public static ValidationRule Currency =
            LambdaConverters.Validator.Create<string>(
                e => decimal.TryParse(e.Value, NumberStyles.Currency,
                    Settings.Default.Culture_SessionCurrency.NumberFormat, out decimal _)
                    ? ValidationResult.ValidResult 
                    : new ValidationResult(false, "ValidationRule_Currency".Localize()));

        public static ValidationRule CurrencyLongRange =
            LambdaConverters.Validator.Create<string>(
                e =>
                {
                    CultureInfo currencyCulture = Settings.Default.Culture_SessionCurrency;
                    decimal factor = (decimal) Math.Pow(10, currencyCulture.NumberFormat.CurrencyDecimalDigits);
                    string message = "ValidationRule_CurrencyLongRange".Localize();
                    message = string.Format(message,
                        (long.MinValue / factor).ToString("C", currencyCulture.NumberFormat),
                        (long.MaxValue / factor).ToString("C", currencyCulture.NumberFormat));
                    decimal outVar;
                    bool parsed = decimal.TryParse(e.Value, NumberStyles.Currency,
                        currencyCulture.NumberFormat, out outVar);
                    if (parsed)
                    {
                        outVar = outVar * factor;
                        if (outVar > long.MaxValue || outVar < long.MinValue)
                            return new ValidationResult(false, message);
                        return ValidationResult.ValidResult;

                    }
                    return new ValidationResult(false, message);
                });

        public static ValidationRule NoEmptyFileName =
            LambdaConverters.Validator.Create<string>(
                e => e.Value != null && !string.IsNullOrEmpty(new FileInfo(e.Value).Name)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "ValidationRule_NotExistingSavePath_EmptyName".Localize()));

        public static ValidationRule NotExistingFilePath =
            LambdaConverters.Validator.Create<string>(
                e => File.Exists(e.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "ValidationRule_NotExistingFilePath".Localize()));

        public static ValidationRule NotExistingSavePath =
            LambdaConverters.Validator.Create<string>(
                e => e.Value != null && Directory.Exists(new FileInfo(e.Value).DirectoryName)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "ValidationRule_NotExistingSavePath".Localize()));
    }
}
