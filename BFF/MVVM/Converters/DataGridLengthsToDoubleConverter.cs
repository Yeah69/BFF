using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace BFF.MVVM.Converters
{
    public class DataGridLengthsToDoubleConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double sum = 0.0;
            foreach (object value in values)
            {
                if (value is DataGridLength)
                    sum += ((DataGridLength)value).DisplayValue;
                else if (value is double)
                    sum += (double)value;
            }
            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
