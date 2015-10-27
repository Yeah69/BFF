using System;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class SumMultiConverter : IMultiValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double sum = (double?)values[0] ?? (double?)values[1] ?? 0.0;
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
