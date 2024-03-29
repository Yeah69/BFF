﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IAccountViewModel : IAccountBaseViewModel
    {
        DateTime StartingDate { get; set; }

        ISumEditViewModel StartingBalanceEdit { get; }
    }

    public interface IImportCsvBankStatement
    {
        ICommand ImportCsvBankStatement { get; }
    }

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    internal sealed class AccountViewModel : AccountBaseViewModel, IAccountViewModel, IImportCsvBankStatement
    {
        private readonly IAccount _account;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IConvertFromTransBaseToTransLikeViewModel _convertFromTransBaseToTransLikeViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICurrentTextsViewModel _currentTextsViewModel;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public DateTime StartingDate
        {
            get => _account.StartingDate;
            set => _account.StartingDate = value;
        }
        public ISumEditViewModel StartingBalanceEdit { get; }

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        public override async Task InsertAsync()
        {
            await base.InsertAsync();
            _summaryAccountViewModel.RefreshStartingBalance();
            _summaryAccountViewModel.RefreshBalance();
        }
        
        public AccountViewModel(
            IAccount account, 
            ISummaryAccountViewModel summaryAccountViewModel,
            Lazy<IAccountViewModelService> accountViewModelService,
            Lazy<IPayeeViewModelService> payeeService,
            Func<IPayee> payeeFactory,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IRxSchedulerProvider rxSchedulerProvider,
            IBackendCultureManager cultureManager,
            ICurrentTextsViewModel currentTextsViewModel,
            IMainWindowDialogManager mainWindowDialogManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            Func<IImportCsvBankStatementViewModel> importCsvBankStatementFactory,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory) 
            : base(
                account,
                accountViewModelService,
                rxSchedulerProvider,
                placeholderFactory,
                convertFromTransBaseToTransLikeViewModel,
                bffSettings,
                cultureManager,
                transDataGridColumnManager)
        {
            _account = account;
            _summaryAccountViewModel = summaryAccountViewModel;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _convertFromTransBaseToTransLikeViewModel = convertFromTransBaseToTransLikeViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;
            _currentTextsViewModel = currentTextsViewModel;

            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            StartingBalanceEdit = createSumEdit(StartingBalance);

            account
                .ObservePropertyChanged(nameof(account.StartingBalance))
                .Subscribe(_ =>
                {
                    summaryAccountViewModel.RefreshStartingBalance();
                    summaryAccountViewModel.RefreshBalance();
                })
                .AddTo(CompositeDisposable);

            account
                .ObservePropertyChanged(nameof(account.StartingBalance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(StartingBalance)))
                .AddTo(CompositeDisposable);

            NewTransactionCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () =>
                    {
                        var transactionViewModel = transactionViewModelFactory(this);
                        if (MissingSum is not null) transactionViewModel.Sum.Value = (long)MissingSum;
                        NewTransList.Add(transactionViewModel);
                    });

            NewTransferCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => NewTransList.Add(transferViewModelFactory(this)));

            NewParentTransactionCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () =>
                    {
                        var parentTransactionViewModel = parentTransactionViewModelFactory(this);
                        NewTransList.Add(parentTransactionViewModel);
                        if (MissingSum is not null)
                        {
                            parentTransactionViewModel.NewSubTransactionCommand.Execute(null);
                            parentTransactionViewModel.NewSubTransactions.First().Sum.Value = (long)MissingSum;
                        }
                    });

            ApplyCommand = RxCommand
                .CallerDeterminedCanExecute(
                    NewTransList
                        .ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                        .Select(count => count > 0),
                    NewTransList.Count > 0)
                .StandardCase(
                    CompositeDisposable,
                    async () => await ApplyTrans());

            ImportCsvBankStatement = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    async () =>
                    {
                        var importCsvBankStatementViewModel = importCsvBankStatementFactory();
                        try
                        {
                            await mainWindowDialogManager.ShowDialogFor(importCsvBankStatementViewModel);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }

                        foreach (var item in importCsvBankStatementViewModel.Items)
                        {
                            var transactionViewModel = transactionViewModelFactory(this);

                            if (item.HasDate)
                                transactionViewModel.Date = item.Date;
                            if (item.HasPayee)
                            {
                                if (payeeService.Value.All?.Any(p => p.Name == item.Payee) ?? false)
                                    transactionViewModel.Payee =
                                        payeeService.Value.All.FirstOrDefault(p => p.Name == item.Payee);
                                else if (item.CreatePayeeIfNotExisting)
                                {
                                    IPayee newPayee = payeeFactory();
                                    newPayee.Name = item.Payee?.Trim() ?? String.Empty;
                                    await newPayee.InsertAsync();
                                    transactionViewModel.Payee = payeeService.Value.GetViewModel(newPayee);
                                }
                            }

                            if (item.HasMemo)
                                transactionViewModel.Memo = item.Memo;
                            if (item.HasSum.Value)
                                transactionViewModel.Sum.Value = item.Sum.Value;

                            NewTransList.Add(transactionViewModel);
                        }
                    });

            if (BffSettings.OpenAccountTab == Name)
                IsOpen = true;
        }

        protected override long? CalculateNewPartOfIntermediateBalance()
        {
            long? sum = 0;
            foreach (var transLikeViewModel in NewTransList)
            {
                switch (transLikeViewModel)
                {
                    case TransferViewModel transfer:
                        sum += transfer.ToAccount == this ? transfer.SumAbsolute : -transfer.SumAbsolute;
                        break;
                    case ParentTransactionViewModel parent:
                        sum += parent.TotalSum.Value;
                        break;
                    default:
                        sum += transLikeViewModel.Sum.Value;
                        break;
                }
            }

            return sum;
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _currentTextsViewModel.CurrentTexts.ConfirmationDialog_Title,
                    _currentTextsViewModel.CurrentTexts.Account_Delete_ConfirmationMessage,
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        foreach (var accountViewModel in AllAccounts ?? Enumerable.Empty<IAccountViewModel>())
                        {
                            accountViewModel.RefreshTransCollection();
                            accountViewModel.RefreshBalance();
                        }
                        _summaryAccountViewModel.RefreshStartingBalance();
                        _summaryAccountViewModel.RefreshBalance();
                        _summaryAccountViewModel.RefreshTransCollection();
                    }
                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }
        
        public override ICommand NewTransactionCommand { get; }
        
        public override ICommand NewTransferCommand { get; }
        
        public override ICommand NewParentTransactionCommand { get; }
        
        public override ICommand ApplyCommand { get; }

        public override ICommand ImportCsvBankStatement { get; }
    }
}
