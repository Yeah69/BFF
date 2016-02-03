using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class SumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long sum = (long)value;
            return sum.AsCurrency(BffEnvironment.CultureProvider.CurrencyCulture);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return ((string) value).CurrencyAsLong(BffEnvironment.CultureProvider.CurrencyCulture);
        }
    }
}
