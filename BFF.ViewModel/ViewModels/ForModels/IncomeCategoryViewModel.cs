﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IIncomeCategoryViewModel : ICategoryBaseViewModel
    {
        int MonthOffset { get; set; }

        void MergeTo(IIncomeCategoryViewModel target);

        bool CanMergeTo(IIncomeCategoryViewModel target);
    }

    public class IncomeCategoryViewModel : CategoryBaseViewModel, IIncomeCategoryViewModel
    {
        private readonly IIncomeCategory _incomeCategory;
        private readonly ILocalizer _localizer;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        public override string FullName => Name;

        public override IEnumerable<ICategoryBaseViewModel> FullChainOfCategories
        {
            get
            {
                yield return this;
            }
        }

        public override int Depth { get; } = 0;

        public override string GetIndent() => "";

        public IncomeCategoryViewModel(
            IIncomeCategory category,
            ILocalizer localizer,
            ISummaryAccountViewModel summaryAccountViewModel,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IAccountViewModelService accountViewModelService,
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
            _incomeCategory = category;
            _localizer = localizer;
            _summaryAccountViewModel = summaryAccountViewModel;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _accountViewModelService = accountViewModelService;
            _rxSchedulerProvider = rxSchedulerProvider;

            category
                .ObservePropertyChanged(nameof(category.MonthOffset))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(MonthOffset)))
                .AddForDisposalTo(CompositeDisposable);
        }

        public int MonthOffset { get => _incomeCategory.MonthOffset; set => _incomeCategory.MonthOffset = value; }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmIncomeCategoryDeletion"), Name),
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

        public void MergeTo(IIncomeCategoryViewModel target)
        {
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmIncomeCategoryMerge"), Name, target.Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(r =>
                {
                    if (r != BffMessageDialogResult.Affirmative) return;

                    if (target is IncomeCategoryViewModel incomeCategoryViewModel)
                    {
                        Observable
                            .FromAsync(token => _incomeCategory.MergeToAsync(incomeCategoryViewModel._incomeCategory))
                            .ObserveOn(_rxSchedulerProvider.UI)
                            .Subscribe(_ =>
                        {
                            _summaryAccountViewModel.RefreshTransCollection();
                            _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                        });
                    }
                });
        }

        public bool CanMergeTo(IIncomeCategoryViewModel target)
        {
            return target.Name != Name;
        }
    }
}