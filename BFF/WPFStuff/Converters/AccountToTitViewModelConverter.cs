using System;
using System.Globalization;
using System.Windows.Data;
using BFF.DB;
using BFF.Model.Native;
using BFF.ViewModel;

namespace BFF.WPFStuff.Converters
{
    public class AccountToTitViewModelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new TitViewModel((IBffOrm)values[1], (Account)values[0]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
