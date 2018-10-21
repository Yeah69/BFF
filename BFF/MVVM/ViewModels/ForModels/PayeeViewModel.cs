using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Core.Helper;
using BFF.Helper.Extensions;
using BFF.Model.Models;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MoreLinq;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel
    {
        void MergeTo(IPayeeViewModel target);
        bool CanMergeTo(IPayeeViewModel target);
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

        public void MergeTo(IPayeeViewModel target)
        {
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    string.Format("ConfirmationDialog_ConfirmPayeeMerge".Localize(), Name, target.Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(r =>
                {
                    if (r != BffMessageDialogResult.Affirmative) return;

                    if (target is PayeeViewModel payeeViewModel)
                    {
                        Observable
                            .FromAsync(token => _payee.MergeToAsync(payeeViewModel._payee), _rxSchedulerProvider.Task)
                            .ObserveOn(_rxSchedulerProvider.UI)
                            .Subscribe(_ =>
                            {
                                _summaryAccountViewModel.RefreshTransCollection();
                                _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                            });
                    }
                });
        }

        public bool CanMergeTo(IPayeeViewModel target)
        {
            return target is PayeeViewModel payeeViewModel
                   && payeeViewModel._payee != _payee
                   && payeeViewModel._payee.Id != _payee.Id;
        }
    }
}