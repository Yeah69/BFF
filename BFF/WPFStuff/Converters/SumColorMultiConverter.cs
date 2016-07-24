using System;
using System.Windows.Data;
using System.Windows.Media;
using BFF.Model.Native;
using BFF.ViewModel.ForModels;

namespace BFF.WPFStuff.Converters
{
    public class SumColorMultiConverter : IMultiValueConverter
    {
        public SolidColorBrush PositiveBrush { get; set; } = Brushes.LimeGreen;

        public SolidColorBrush NegativeBrush { get; set; } = Brushes.DarkOrange;

        public SolidColorBrush TransferBrush { get; set; } = Brushes.RoyalBlue;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Transfer in "All Accounts"-Tab
            if (values[0] is AllAccounts)
                return TransferBrush;
            Account account = (Account)values[0];
            //Transfer in FromAccount-Tab
            if(account == (Account)values[1])
                return NegativeBrush;
            //Transfer in ToAccount-Tab
            if (account == (Account)values[2])
                return PositiveBrush;
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
