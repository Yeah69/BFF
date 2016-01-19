using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class SumColorMultiConverter : IMultiValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Not a Transfer
            if(values[2] == DependencyProperty.UnsetValue && values[3] == DependencyProperty.UnsetValue)
                return (long)values[0] < 0L ? NegativeBrush : PositiveBrush;
            //Transfer in "All Accounts"-Tab
            if (values[1] == null)
                return TransferBrush;
            Account account = (Account)values[1];
            //Transfer in FromAccount-Tab
            if(account == (Account)values[2])
                return NegativeBrush;
            //Transfer in ToAccount-Tab
            if (account == (Account)values[3])
                return PositiveBrush;
            return Brushes.Transparent;
            //todo: Enhance possibility of Acount-specific conversion; Transfer: if specific Account is FromAccount then negative color, else positive
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
