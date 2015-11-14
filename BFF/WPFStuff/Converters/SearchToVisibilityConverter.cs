using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class SearchToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string searchString = (string) parameter ?? "";
            return value.ToString().Contains(searchString) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           throw new NotImplementedException();
        }
    }
}
