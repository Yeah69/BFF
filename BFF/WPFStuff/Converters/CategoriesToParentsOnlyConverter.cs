using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BFF.Helper;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class CategoriesToParentsOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<Category> enumerable = (IEnumerable<Category>)value;
            return enumerable.Where(category => category.Parent == null);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return ((string) value).CurrencyAsLong(BffEnvironment.CultureProvider.CurrencyCulture);
        }
    }
}
