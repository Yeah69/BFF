using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BFF.WPFStuff.Converters
{
    public class SumMultiConverter : IMultiValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(values[0] == DependencyProperty.UnsetValue)
                return Brushes.Gold;
            long sum = (long?)values[0] ?? (long?)values[1] ?? 0L;
            if ((string)values[2] == "Transfer")
            {
                return TransferBrush;
            }
            return sum < 0.0 ? NegativeBrush : PositiveBrush;
            //todo: Enhance possibility of Acount-specific conversion; Transfer: if specific Account is FromAccount then negative color, else positive
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
