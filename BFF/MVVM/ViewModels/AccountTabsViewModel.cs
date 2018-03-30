using System.Globalization;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
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

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts =>
            _accountViewModelService.All;

        public ISummaryAccountViewModel SummaryAccountViewModel { get; }

        public INewAccountViewModel NewAccountViewModel { get; }

        public AccountTabsViewModel(
            INewAccountViewModel newAccountViewModel,
            IAccountViewModelService accountViewModelService,
            ISummaryAccountViewModel summaryAccountViewModel)
        {
            _accountViewModelService = accountViewModelService;
            SummaryAccountViewModel = summaryAccountViewModel;
            NewAccountViewModel = newAccountViewModel;

            IsOpen.Value = true;
        }

        protected override void OnIsOpenChanged(bool isOpen)
        {
            if (isOpen && SummaryAccountViewModel.IsOpen.Value)
            {
                Messenger.Default.Send(SummaryAccountMessage.Refresh);
            }
        }
    }
}
