﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MoreLinq;
using MrMeeseeks.ResXToViewModelGenerator;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel
    {
        void MergeTo(IPayeeViewModel target);
        bool CanMergeTo(IPayeeViewModel target);
    }

    internal class PayeeViewModel : CommonPropertyViewModel, IPayeeViewModel
    {
        public IAccountViewModelService AccountViewModelService { get; }
        private readonly IPayee _payee;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ICurrentTextsViewModel _currentTextsViewModel;

        public PayeeViewModel(
            IPayee payee,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            ISummaryAccountViewModel summaryAccountViewModel,
            IAccountViewModelService accountViewModelService,
            ICurrentTextsViewModel currentTextsViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(payee, rxSchedulerProvider)
        {
            AccountViewModelService = accountViewModelService;
            _payee = payee;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _summaryAccountViewModel = summaryAccountViewModel;
            _accountViewModelService = accountViewModelService;
            _currentTextsViewModel = currentTextsViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _currentTextsViewModel.CurrentTexts.ConfirmationDialog_Title,
                    string.Format(
                        _currentTextsViewModel.CurrentTexts.ConfirmationDialog_ConfirmPayeeDeletion, 
                        Name),
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
                    _currentTextsViewModel.CurrentTexts.ConfirmationDialog_Title,
                    string.Format(
                        _currentTextsViewModel.CurrentTexts.ConfirmationDialog_ConfirmPayeeMerge, 
                        Name, 
                        target.Name),
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
            return target.Name != Name;
        }
    }
}