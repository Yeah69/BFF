using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WPFLocalizeExtension.Extensions;

namespace BFF.WPFStuff.Converters
{
    public class NullToLexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
            {
                LocTextExtension ext = new LocTextExtension("All_Accounts");
                ext.SetBinding(values[1], TextBlock.TextProperty);
            }
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
