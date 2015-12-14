using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BFF.WPFStuff.Converters
{
    public class RootNodesOnlyConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a flat IEnumerable of nodes from trees to the root nodes only
        /// </summary>
        /// <param name="values">[0]: the flat list; [1]: parent property name</param>
        /// <param name="targetType">interface requirement, not used</param>
        /// <param name="parameter">interface requirement, not used</param>
        /// <param name="culture">interface requirement, not used</param>
        /// <returns>List of root nodes only</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<object> allCategories = (IEnumerable<object>)values[0];
            string parentName = (string) values[1];

            return allCategories?.Where(item =>
            {
                if (item.GetType().GetProperty(parentName) == null)
                    return false;
                return item.GetType().GetProperty(parentName).GetValue(item) == null;
            });
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(RootNodesOnlyConverter)} does not need a ConvertBack. But it still was called.");
        }
    }
}
