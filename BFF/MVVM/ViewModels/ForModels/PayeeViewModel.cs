using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MoreLinq;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel
    {
        IRxRelayCommand<IPayeeViewModel> MergeTo { get; }
    }

    public class PayeeViewModel : CommonPropertyViewModel, IPayeeViewModel
    {
        public IAccountViewModelService AccountViewModelService { get; }
        private readonly IPayee _payee;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountViewModelService _accountViewModelService;

        public PayeeViewModel(
            IPayee payee,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            ISummaryAccountViewModel summaryAccountViewModel,
            IAccountViewModelService accountViewModelService,
            IRxSchedulerProvider rxSchedulerProvider) : base(payee, rxSchedulerProvider)
        {
            AccountViewModelService = accountViewModelService;
            _payee = payee;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _summaryAccountViewModel = summaryAccountViewModel;
            _accountViewModelService = accountViewModelService;
            _rxSchedulerProvider = rxSchedulerProvider;

            MergeTo = new RxRelayCommand<IPayeeViewModel>(cvm =>
            {
                if (cvm is PayeeViewModel payeeViewModel)
                {
                    payee.MergeTo(payeeViewModel._payee);
                }
            });
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    string.Format("ConfirmationDialog_ConfirmPayeeDeletion".Localize(), Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                        _summaryAccountViewModel.RefreshTransCollection();
                    }
                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }

        public IRxRelayCommand<IPayeeViewModel> MergeTo { get; }
    }
}