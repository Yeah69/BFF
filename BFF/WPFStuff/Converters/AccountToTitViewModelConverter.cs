using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using BFF.DB;
using BFF.Helper;
using BFF.Model.Native;
using BFF.ViewModel;
using Ninject;
using Ninject.Parameters;

namespace BFF.WPFStuff.Converters
{
    public class AccountToTitViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StandardKernel kernel = new StandardKernel(new BffNinjectModule());
            return (from account in (IEnumerable<Account>) value select kernel.Get<TitViewModel>(new ConstructorArgument("account", account))).Cast<object>().ToList();
            return value == null
                ? kernel.Get<TitViewModel>()
                : kernel.Get<TitViewModel>(new ConstructorArgument("account", (Account) value));
                //new TitViewModel((IBffOrm)parameter, (Account)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
