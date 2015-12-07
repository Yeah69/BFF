using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class RootCategoriesOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<Category> allCategories = (ObservableCollection<Category>) value;
            return allCategories?.Where(category => category.Parent == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(RootCategoriesOnlyConverter)} does not need a ConvertBack. But it still was called.");
        }
    }
}
