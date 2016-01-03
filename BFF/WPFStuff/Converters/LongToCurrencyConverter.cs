using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class LongToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((long) value).AsCurrency(Output.CurrencyCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           decimal decval = decimal.Parse((string)value, NumberStyles.Currency, Output.CurrencyCulture.NumberFormat);
            return (long)(decval * 100);
            //return ((string) value).CurrencyAsLong();
        }
    }
}
