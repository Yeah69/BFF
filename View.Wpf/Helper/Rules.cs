using BFF.Properties;
using BFF.ViewModel.ViewModels.ForModels;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BFF.View.Wpf.Helper
{
    internal static class Rules
    {
        public static readonly ValidationRule EmptyPayee =
            LambdaConverters.Validator.Create<IPayeeViewModel?>(
                e => e.Value is not null
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ErrorMessageEmptyPayee));

        public static readonly ValidationRule EmptyCategory =
            LambdaConverters.Validator.Create<ICategoryBaseViewModel?>(
                e => e.Value is not null
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ErrorMessageEmptyCategory));

        public static readonly ValidationRule Currency =
            LambdaConverters.Validator.Create<string>(
                e => decimal.TryParse(e.Value, NumberStyles.Currency,
                    Settings.Default.Culture_SessionCurrency.NumberFormat, out decimal _)
                    ? ValidationResult.ValidResult 
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ValidationRule_Currency));

        public static readonly ValidationRule CurrencyLongRange =
            LambdaConverters.Validator.Create<string>(
                e =>
                {
                    CultureInfo currencyCulture = Settings.Default.Culture_SessionCurrency;
                    decimal factor = (decimal) Math.Pow(10, currencyCulture.NumberFormat.CurrencyDecimalDigits);
                    string message = ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ValidationRule_CurrencyLongRange;
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

        public static readonly ValidationRule NoEmptyFileName =
            LambdaConverters.Validator.Create<string?>(
                e => e.Value is not null && !string.IsNullOrEmpty(new FileInfo(e.Value).Name)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ValidationRule_NotExistingSavePath_EmptyName));

        public static readonly ValidationRule NotExistingFilePath =
            LambdaConverters.Validator.Create<string>(
                e => File.Exists(e.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ValidationRule_NotExistingFilePath));

        public static readonly ValidationRule NotExistingSavePath =
            LambdaConverters.Validator.Create<string?>(
                e => e.Value is not null && Directory.Exists(new FileInfo(e.Value).DirectoryName)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, ((App) Application.Current).CurrentTextsViewModel.CurrentTexts.ValidationRule_NotExistingSavePath));
    }
}
