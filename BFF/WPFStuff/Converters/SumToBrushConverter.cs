using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BFF.WPFStuff.Converters
{
    public class SumToBrushConverter : IValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.LimeGreen;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (long) value < 0L ? NegativeBrush : PositiveBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
