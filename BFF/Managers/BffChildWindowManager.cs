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
        private readonly Func<INewFileAccessDialog> _newFileAccessDialogFactory;
        private readonly Func<IOpenFileAccessDialog> _openFileAccessDialogFactory;

        public BffChildWindowManager(
            Lazy<MainWindow> lazyMainWindow,
            Func<IImportCsvBankStatementView> importCsvBankStatementViewFactory,
            Func<INewFileAccessDialog> newFileAccessDialogFactory,
            Func<IOpenFileAccessDialog> openFileAccessDialogFactory)
        {
            _lazyMainWindow = lazyMainWindow;
            _importCsvBankStatementViewFactory = importCsvBankStatementViewFactory;
            _newFileAccessDialogFactory = newFileAccessDialogFactory;
            _openFileAccessDialogFactory = openFileAccessDialogFactory;
        }

        public async Task<bool> OpenOkCancelDialog(IOkCancelDialogViewModel dataContext)
        {
            var childWindow =
                dataContext switch
                    {
                    INewFileAccessViewModel _ => _newFileAccessDialogFactory() as ChildWindow,
                    IOpenFileAccessViewModel _ => _openFileAccessDialogFactory() as ChildWindow,
                    IImportCsvBankStatementViewModel _ => _importCsvBankStatementViewFactory() as ChildWindow,
                    _ => throw new ArgumentException("Not supported ViewModel-type!")
                    };
            childWindow = childWindow ?? throw new ArgumentException($"Couldn't create {nameof(ChildWindow)} from given ViewModel.");
            childWindow.DataContext = dataContext;
            await _lazyMainWindow.Value.ShowChildWindowAsync(childWindow);
            return dataContext.Result == OkCancelResult.Ok;
        }
    }
}
