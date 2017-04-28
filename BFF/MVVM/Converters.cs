using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Helper;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Properties;
using LambdaConverters;

namespace BFF.MVVM
{
    internal static class Converters
    {
        //Fields and Properties

        private static readonly long CurrencyComboSampleValue = 123457869L;

        private static readonly DateTime DateComboSampleValue = new DateTime(2013, 9, 6);

        private static SolidColorBrush TransactionBrush => (SolidColorBrush)Application.Current.TryFindResource("TransactionBrush") ?? Brushes.DarkOrange;

        private static SolidColorBrush IncomeBrush => (SolidColorBrush)Application.Current.TryFindResource("IncomeBrush") ?? Brushes.LimeGreen;

        private static SolidColorBrush TransferBrush => (SolidColorBrush)Application.Current.TryFindResource("TransferBrush") ?? Brushes.RoyalBlue;

        //Single Value Converters

        /// <summary>
        /// True for long date-picker format, false for short. Has convert back function.
        /// </summary>
        public static readonly IValueConverter BooleanToDatePickerFormat =
            ValueConverter.Create<bool, DatePickerFormat>(
                e => e.Value ? DatePickerFormat.Long : DatePickerFormat.Short,
                e => e.Value == DatePickerFormat.Long);

        /// <summary>
        /// Convert back function returns false for "null" references. Otherwise, it works as expected.
        /// </summary>
        public static readonly IValueConverter BooleanToNullableBoolean =
            ValueConverter.Create<bool, bool?>(
                e => e.Value, 
                e => e.Value ?? false);

        /// <summary>
        /// A helping converter for the listing of all currencies. The country code is converted to a CultureInfo and from that example strings for the country's currency are derived.
        /// The parameter determines the sign before the number. No convert back function.
        /// </summary>
        public static readonly IValueConverter CurrencyChoiceBoxExampleConversion =
            ValueConverter.Create<string, string, string>(
                e => 
                {
                    var itemCulture = CultureInfo.GetCultureInfo(e.Value);
                    return e.Parameter == "negative" 
                    ? (-CurrencyComboSampleValue).AsCurrency(itemCulture) 
                    : CurrencyComboSampleValue.AsCurrency(itemCulture);
                });

        /// <summary>
        /// A helping converter for the listing of all date formats. The country code is converted to a CultureInfo and from that example strings for the country's date format are derived.
        /// The parameter chooses between long and short format. No convert back function.
        /// </summary>
        public static readonly IValueConverter DateChoiceBoxExampleConversion =
            ValueConverter.Create<string, string, string>(
                e => 
                {
                    var itemCulture = CultureInfo.GetCultureInfo(e.Value);
                    return e.Parameter == "long" 
                    ? DateComboSampleValue.ToString(itemCulture.DateTimeFormat.LongDatePattern, itemCulture.DateTimeFormat)
                    : DateComboSampleValue.ToString(itemCulture.DateTimeFormat.ShortDatePattern, itemCulture.DateTimeFormat);
                });

        /// <summary>
        /// Converts a DateTime value to a string. No convert back function.
        /// </summary>
        public static readonly IValueConverter DateTimeToString =
            ValueConverter.Create<DateTime, string>(
                e => e.Value.ToString(
                    Settings.Default.Culture_DefaultDateLong
                    ? Settings.Default.Culture_SessionDate.DateTimeFormat.LongDatePattern
                    : Settings.Default.Culture_SessionDate.DateTimeFormat.ShortDatePattern
                    , Settings.Default.Culture_SessionDate.DateTimeFormat),
                e => DateTime.Parse(e.Value, Settings.Default.Culture_SessionDate.DateTimeFormat));

        /// <summary>
        /// Converts a sum (which can also be null) to a string. The sum is a nullable long. Hence it is not decimal. 
        /// Only the string represents the sum as decimal depending on the currently set CultureInfo for currencies.
        /// Has convert back function.
        /// </summary>
        public static readonly IValueConverter NullableSumToString =
            ValueConverter.Create<long?, string>(
                e => e.Value == null
                         ? ""
                         : ((long) e.Value).AsCurrency(Settings.Default.Culture_SessionCurrency),
                e => e.Value.CurrencyAsLong(Settings.Default.Culture_SessionCurrency));

        /// <summary>
        /// Negative sums get the same color as the transactions and positive sums (and zero) get the same color as incomes.
        /// Because it is expected that most of the transactions get negative sums and most of the incomes get positive sums.
        /// No convert back function.
        /// </summary>
        public static readonly IValueConverter SumToSolidColorBrush =
            ValueConverter.Create<long, SolidColorBrush>(
                e => e.Value < 0L ? TransactionBrush : IncomeBrush);

        //Multi Value Converters

        /// <summary>
        /// Calculates the cumulative length of given element widths. No convert back function.
        /// </summary>
        public static readonly IMultiValueConverter WidthsToDouble =
            MultiValueConverter.Create<object, double>(
                e =>
                {
                    double sum = 0.0;
                    foreach (object value in e.Values)
                    {
                        if (value is DataGridLength)
                            sum += ((DataGridLength)value).DisplayValue;
                        else if (value is double)
                            sum += (double)value;
                    }
                    return sum;
                });

        /// <summary>
        /// Calculates the cumulative length of given element widths and returns the value as a left margin. No convert back function.
        /// </summary>
        public static readonly IMultiValueConverter WidthsToLeftMargin =
            MultiValueConverter.Create<object, Thickness>(
                e =>
                {
                    double sum = 0.0;
                    foreach (object value in e.Values)
                    {
                        if (value is DataGridLength)
                            sum += ((DataGridLength)value).DisplayValue;
                        else if (value is double)
                            sum += (double)value;
                    }
                    return new Thickness(sum, 0.0, 0.0, 0.0);
                });

        /// <summary>
        /// This converter should be applied on three specific bindings:
        /// 1. The AccountViewModel of the current tab.
        /// 2. The FromAccount of the transfer.
        /// 3. The Sum of the transfer.
        /// If the current tab is the one where the money was transferred from, then it should be shown with a negative sign, otherwise positive.
        /// No convert back function.
        /// </summary>
        public static readonly IMultiValueConverter TransferSumAsCorrectlySignedString =
            MultiValueConverter.Create<object, string>(
                e => e.Values[0] == e.Values[1]
                ? (-1 * (long)e.Values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency) 
                : ((long)e.Values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency));

        /// <summary>
        /// The transfer color depends on the currently currently shown Account. Therefore this converter is applied on three specific bindings:
        /// 1. Currently shown AccountViewModel
        /// 2. FromAccount-ViewModel of the transfer
        /// 3. ToAccount-ViewModel of the transfer
        /// All transfers in the summary account get the neutral transfer color. 
        /// Transfers in shown in their FromAccount get the transaction color, because they are negative.
        /// Transfers in shown in their ToAccount get the income color, because they are positive.
        /// No convert back function.
        /// </summary>
        public static readonly IMultiValueConverter TransferSumToSolidColorBrush =
            MultiValueConverter.Create<IAccountBaseViewModel, SolidColorBrush>(
                e =>
                {
                    //Transfer in "All Accounts"-Tab
                    if (e.Values[0] is ISummaryAccountViewModel)
                        return TransferBrush;
                    IAccountViewModel account = (IAccountViewModel)e.Values[0];
                    //Transfer in FromAccount-Tab
                    if (account == (IAccountViewModel)e.Values[1])
                        return TransactionBrush;
                    //Transfer in ToAccount-Tab
                    if (account == (IAccountViewModel)e.Values[2])
                        return IncomeBrush;
                    return Brushes.Transparent; //Error case
                });
    }
}
