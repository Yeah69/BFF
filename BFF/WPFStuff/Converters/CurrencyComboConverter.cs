using System;
using System.Globalization;
using System.Windows.Data;
using BFF.Helper;

namespace BFF.WPFStuff.Converters
{
    public class CurrencyComboConverter : IValueConverter
    {
        public const long SampleValue = 123457869L;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemCulture = CultureInfo.GetCultureInfo((string) value);
            return (string) parameter == "negative" ? (-SampleValue).AsCurrency(itemCulture) : SampleValue.AsCurrency(itemCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           throw new NotImplementedException();
        }
    }
}
