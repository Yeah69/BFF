using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class DepPropToUnsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? ((long) value).AsCurrency(Output.CurrencyCulture) : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).CurrencyAsLong(Output.CurrencyCulture);
        }
    }
}
