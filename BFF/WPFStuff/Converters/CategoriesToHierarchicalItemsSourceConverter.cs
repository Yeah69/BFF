using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BFF.Helper;
using BFF.Model.Native;

namespace BFF.WPFStuff.Converters
{
    public class CategoriesToHierarchicalItemsSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<Category> enumerable = (IEnumerable<Category>)value;
            IEnumerable<Category> parentCategories = enumerable.Where(category => category.Parent == null);
            IList<Category> ret = new List<Category>();
            foreach (Category parentCategory in parentCategories)
            {
                ret.Add(parentCategory);
                fillWithDescandents(parentCategory, ret);
            }
            return ret;
        }

        private void fillWithDescandents(Category parentCategory, IList<Category> list)
        {
            foreach (Category childCategory in parentCategory.Categories)
            {
                list.Add(childCategory);
                fillWithDescandents(childCategory, list);
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return ((string) value).CurrencyAsLong(BffEnvironment.CultureProvider.CurrencyCulture);
        }
    }
}
