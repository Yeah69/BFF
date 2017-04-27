using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BFF.Helper;
using BFF.Properties;
using LambdaConverters;

namespace BFF.MVVM
{
    internal static class Converter
    {
        public static readonly IValueConverter BooleanToDatePickerFormat =
            ValueConverter.Create<bool, DatePickerFormat>(
                e => e.Value ? DatePickerFormat.Long : DatePickerFormat.Short,
                e => e.Value == DatePickerFormat.Long);

        public static readonly IValueConverter BooleanToNullableBoolean =
            ValueConverter.Create<bool, bool?>(
                e => e.Value, 
                e => e.Value ?? false);

        private static readonly long CurrencyComboSampleValue = 123457869L;

        public static readonly IValueConverter CurrencyCombo =
            ValueConverter.Create<string, string, string>(
                e => 
                {
                    var itemCulture = CultureInfo.GetCultureInfo(e.Value);
                    return e.Parameter == "negative" 
                    ? (-CurrencyComboSampleValue).AsCurrency(itemCulture) 
                    : CurrencyComboSampleValue.AsCurrency(itemCulture);
                });

        private static readonly DateTime DateComboSampleValue = new DateTime(2013, 9, 6);

        public static readonly IValueConverter DateCombo =
            ValueConverter.Create<string, string, string>(
                e => 
                {
                    var itemCulture = CultureInfo.GetCultureInfo(e.Value);
                    return e.Parameter == "long" 
                    ? DateComboSampleValue.ToString(itemCulture.DateTimeFormat.LongDatePattern, itemCulture.DateTimeFormat)
                    : DateComboSampleValue.ToString(itemCulture.DateTimeFormat.ShortDatePattern, itemCulture.DateTimeFormat);
                });

        public static readonly IValueConverter DateToString =
            ValueConverter.Create<DateTime, string>(
                e => e.Value.ToString(
                    Settings.Default.Culture_DefaultDateLong
                    ? Settings.Default.Culture_SessionDate.DateTimeFormat.LongDatePattern
                    : Settings.Default.Culture_SessionDate.DateTimeFormat.ShortDatePattern
                    , Settings.Default.Culture_SessionDate.DateTimeFormat),
                e => DateTime.Parse(e.Value, Settings.Default.Culture_SessionDate.DateTimeFormat));

        public static readonly IValueConverter Sum =
            ValueConverter.Create<long?, string>(
                e => e.Value == null
                         ? ""
                         : ((long) e.Value).AsCurrency(Settings.Default.Culture_SessionCurrency),
                e => e.Value.CurrencyAsLong(Settings.Default.Culture_SessionCurrency));

        public static readonly IMultiValueConverter DataGridLengthsToDouble =
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

        public static readonly IMultiValueConverter DataGridWidthsToLeftMargin =
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

        public static readonly IMultiValueConverter NegativeTransfer =
            MultiValueConverter.Create<object, string>(
                e =>
                {
                    //Transfer is in FromAccount
                    if (e.Values[0] == e.Values[1])
                        return (-1 * (long)e.Values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency);
                    return ((long)e.Values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency);
                });
    }
}
