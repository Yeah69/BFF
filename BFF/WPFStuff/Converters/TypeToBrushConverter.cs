using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class TypeToBrushConverter : IValueConverter
    {
        public SolidColorBrush TransactionBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush IncomeBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            if ((string)value == "Transfer") return TransferBrush;
            if (((string)value).EndsWith("Trans")) return TransactionBrush;
            return ((string)value).EndsWith("Income") ? IncomeBrush : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
