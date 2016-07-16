using System;
using System.Windows.Data;
using BFF.Helper;
using BFF.Properties;

namespace BFF.WPFStuff.Converters
{
    public class NegativeTransferMultiConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Transfer is in FromAccount
            if (values[0] == values[1])
                return (-1 * (long) values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency);
            return  ((long)values[2]).AsCurrency(Settings.Default.Culture_SessionCurrency);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
