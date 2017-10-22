using System.Windows.Controls;
using BFF.Helper.Extensions;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM
{
    internal static class Rules
    {
        public static ValidationRule EmptyPayee =
            LambdaConverters.Validator.Create<IPayeeViewModel>(
                e => e.Value != null ? ValidationResult.ValidResult : new ValidationResult(false, "ErrorMessageEmptyPayee".Localize<string>()));

        public static ValidationRule EmptyCategory =
            LambdaConverters.Validator.Create<ICategoryViewModel>(
                e => e.Value != null ? ValidationResult.ValidResult : new ValidationResult(false, "ErrorMessageEmptyCategory".Localize<string>()));
    }
}
