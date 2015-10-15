using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class SumToBrushConverter : IValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            if (typeof (Transfer) == (Type) parameter) return TransferBrush;
            if ((double) value < 0.0)
                return NegativeBrush;
            return PositiveBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
