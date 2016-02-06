using System;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class NegativeTransferMultiConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Transfer is in FromAccount
            if (values[0] == values[1])
                return (-1 * (long) values[2]).AsCurrency(BffEnvironment.CultureProvider.CurrencyCulture);
            return  ((long)values[2]).AsCurrency(BffEnvironment.CultureProvider.CurrencyCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
