using System;
using System.Globalization;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class BooleanToNullableBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            return (bool) value;
        }
    }
}
