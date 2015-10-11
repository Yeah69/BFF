using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BFF.WPFStuff.Converters
{
    public class SumToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            if ((double) value < 0.0)
                return Brushes.DarkOrange;
            return Brushes.LimeGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
