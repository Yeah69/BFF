using System;
using System.Globalization;
using System.Windows.Data;
using BFF.DB;
using BFF.Helper;
using BFF.Model.Native;
using BFF.ViewModel;
using Ninject;

namespace BFF.WPFStuff.Converters
{
    public class AccountToTitViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StandardKernel kernel = new StandardKernel(new BffNinjectModule());
            //return (from account in (IEnumerable<Account>) value select kernel.Get<TitViewModel>(new ConstructorArgument("account", account))).Cast<object>().ToList();
            return new TitViewModel(kernel.Get<IBffOrm>(), (Account)value);
            //return value == null
            //    ? kernel.Get<TitViewModel>()
            //    : kernel.Get<TitViewModel>(new ConstructorArgument("account", (Account) value));
                //new TitViewModel((IBffOrm)parameter, (Account)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
