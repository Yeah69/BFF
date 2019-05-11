using BFF.Core.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    public interface IAccountTabsViewModel
    {
        INewAccountViewModel NewAccountViewModel { get; }
        IObservableReadOnlyList<IAccountViewModel> AllAccounts { get; }
        ISummaryAccountViewModel SummaryAccountViewModel { get; }
        IReactiveProperty<bool> IsOpen { get; }
    }

    public class AccountTabsViewModel : SessionViewModelBase, IAccountTabsViewModel
    {
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly IBffSettings _bffSettings;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts =>
            _accountViewModelService.All;

        public ISummaryAccountViewModel SummaryAccountViewModel { get; }

        public INewAccountViewModel NewAccountViewModel { get; }

        public AccountTabsViewModel(
            INewAccountViewModel newAccountViewModel,
            IAccountViewModelService accountViewModelService,
            ISummaryAccountViewModel summaryAccountViewModel,
            IBffSettings bffSettings)
        {
            _accountViewModelService = accountViewModelService;
            _bffSettings = bffSettings;
            SummaryAccountViewModel = summaryAccountViewModel;
            NewAccountViewModel = newAccountViewModel;

            IsOpen.Value = _bffSettings.OpenMainTab == "Accounts";
        }

        protected override void OnIsOpenChanged(bool isOpen)
        {
            if (isOpen && SummaryAccountViewModel.IsOpen)
            {
                SummaryAccountViewModel.RefreshTransCollection();
                SummaryAccountViewModel.RefreshStartingBalance();
                SummaryAccountViewModel.RefreshBalance();
            }
            if(IsOpen.Value)
            {
                _bffSettings.OpenMainTab = "Accounts";
            }
        }
    }
}
