using System;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.Views;
using BFF.Views.Dialogs;
using MahApps.Metro.SimpleChildWindow;

namespace BFF.Managers
{
    public class BffChildWindowManager : IBffChildWindowManager, IOncePerApplication
    {
        private readonly Lazy<MainWindow> _lazyMainWindow;
        private readonly Func<IImportCsvBankStatementView> _importCsvBankStatementViewFactory;

        public BffChildWindowManager(
            Lazy<MainWindow> lazyMainWindow,
            Func<IImportCsvBankStatementView> importCsvBankStatementViewFactory)
        {
            _lazyMainWindow = lazyMainWindow;
            _importCsvBankStatementViewFactory = importCsvBankStatementViewFactory;
        }

        public Task OpenImportCsvBankStatementDialogAsync(IImportCsvBankStatementViewModel dataContext)
        {
            if (_importCsvBankStatementViewFactory() is ChildWindow childWindow)
            {
                childWindow.DataContext = dataContext;
                dataContext.IsOpen.Value = true;
                return _lazyMainWindow.Value.ShowChildWindowAsync(childWindow);
            }

            return Task.CompletedTask;
        }
    }
}
