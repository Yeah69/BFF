using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Helper;
using BFF.Properties;

namespace BFF.MVVM.Converters
{
    public class SumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            long sum = (long)value;
            return sum.AsCurrency(Settings.Default.Culture_SessionCurrency);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return ((string) value).CurrencyAsLong(Settings.Default.Culture_SessionCurrency);
        }
    }
}
