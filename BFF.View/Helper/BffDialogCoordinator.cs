using System;
using System.Threading.Tasks;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace BFF.View.Helper
{

    class MainDialogCoordinator : IMainBffDialogCoordinator
    {
        private readonly Lazy<IMainWindowViewModel> _lazyMainWindowViewModel;
        private readonly IDialogCoordinator _dialogCoordinator;

        public MainDialogCoordinator(
            Lazy<IMainWindowViewModel> lazyMainWindowViewModel,
            IDialogCoordinator dialogCoordinator)
        {
            _lazyMainWindowViewModel = lazyMainWindowViewModel;
            _dialogCoordinator = dialogCoordinator;
        }

        public async Task<BffMessageDialogResult> ShowMessageAsync(string title, string message, BffMessageDialogStyle style = BffMessageDialogStyle.Affirmative)
        {
            MessageDialogStyle mahAppsDialogStyle = MessageDialogStyle.Affirmative;

            switch (style)
            {
                case BffMessageDialogStyle.Affirmative:
                    mahAppsDialogStyle = MessageDialogStyle.Affirmative;
                    break;
                case BffMessageDialogStyle.AffirmativeAndNegative:
                    mahAppsDialogStyle = MessageDialogStyle.AffirmativeAndNegative;
                    break;
            }

            var dialogResult = await _dialogCoordinator.ShowMessageAsync(_lazyMainWindowViewModel.Value, title, message, mahAppsDialogStyle).ConfigureAwait(false);

            switch (dialogResult)
            {
                case MessageDialogResult.Affirmative:
                    return BffMessageDialogResult.Affirmative;
                case MessageDialogResult.Negative:
                    return BffMessageDialogResult.Negative;
                default:
                    return BffMessageDialogResult.Negative;
            }
        }
    }
}
