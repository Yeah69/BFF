using BFF.View.Wpf.Views;
using BFF.View.Wpf.Views.Dialogs;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;

namespace BFF.View.Wpf.Managers
{
    internal class MainWindowDialogManager : IMainWindowDialogManager
    {
        private readonly MainWindow _mainWindow;
        private readonly Func<IMainWindowDialogContentViewModel, IMainWindowDialogViewModel> _mainWindowDialogViewModelFactory;
        private readonly Func<IMainWindowDialogViewModel, MainWindowDialogView> _mainWindowDialogViewFactory;

        public MainWindowDialogManager(
            MainWindow mainWindow,
            Func<IMainWindowDialogContentViewModel, IMainWindowDialogViewModel> mainWindowDialogViewModelFactory,
            Func<IMainWindowDialogViewModel, MainWindowDialogView> mainWindowDialogViewFactory)
        {
            _mainWindow = mainWindow;
            _mainWindowDialogViewModelFactory = mainWindowDialogViewModelFactory;
            _mainWindowDialogViewFactory = mainWindowDialogViewFactory;
        }

        public async Task<T> ShowDialogFor<T>(IMainWindowDialogContentViewModel<T> contentViewModel)
        {
            var viewModel = _mainWindowDialogViewModelFactory(contentViewModel);
            var view = _mainWindowDialogViewFactory(viewModel);
            await _mainWindow.ShowMetroDialogAsync(view);
            try
            {
                await contentViewModel.Result;
            }
            catch (OperationCanceledException)
            {
                // ignore cancellation
            }
            await _mainWindow.HideMetroDialogAsync(view);
            return await contentViewModel.Result;
        }
    }
}