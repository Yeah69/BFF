using System;
using System.Threading.Tasks;
using BFF.MVVM.ViewModels.Dialogs;
using BFF.MVVM.Views;
using BFF.MVVM.Views.Dialogs;
using MahApps.Metro.SimpleChildWindow;

namespace BFF.MVVM.Managers
{
    public interface IBffChildWindowManager
    {
        Task OpenImportCsvBankStatementDialogAsync(IImportCsvBankStatementViewModel dataContext);
    }

    public class BffChildWindowManager : IBffChildWindowManager
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
