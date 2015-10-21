using System;
using System.Linq;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class CoalesceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values?.FirstOrDefault(item => item != null);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
