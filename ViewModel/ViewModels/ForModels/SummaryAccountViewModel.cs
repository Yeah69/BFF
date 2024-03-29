﻿using System;
using System.Linq;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface ISummaryAccountViewModel : IAccountBaseViewModel {
        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        void RefreshStartingBalance();
    }

    /// <summary>
    /// Trans can be added to an Account
    /// </summary>
    internal sealed class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel, IOncePerBackend
    {
        private readonly Lazy<IAccountViewModelService> _service;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override string Name { get; set; }
        
        public SummaryAccountViewModel(
            ISummaryAccount summaryAccount, 
            Lazy<IAccountViewModelService> service,
            IBffSettings bffSettings,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IRxSchedulerProvider rxSchedulerProvider,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IBackendCultureManager cultureManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            ICurrentTextsViewModel currentTextsViewModel,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory) 
            : base(
                summaryAccount,
                service,
                rxSchedulerProvider,
                placeholderFactory,
                convertFromTransBaseToTransLikeViewModel,
                bffSettings,
                cultureManager,
                transDataGridColumnManager)
        {
            _service = service;

            if(string.IsNullOrWhiteSpace(bffSettings.OpenAccountTab))
                IsOpen = true;

            Name = currentTextsViewModel.CurrentTexts.AllAccounts;
            
            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);

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
                            parentTransactionViewModel.NewSubTransactionCommand?.Execute(null);
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

            ImportCsvBankStatement = RxCommand.CanNeverExecute();

            RefreshStartingBalance();
            RefreshBalance();
        }

        #region ViewModel_Part

        protected override long? CalculateNewPartOfIntermediateBalance()
        {
            long? sum = 0;
            foreach (var transLikeViewModel in NewTransList)
            {
                switch (transLikeViewModel)
                {
                    case TransferViewModel _:
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

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public sealed override ICommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public sealed override ICommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public sealed override ICommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        public sealed override ICommand ApplyCommand { get; }

        public override ICommand ImportCsvBankStatement { get; }

        #endregion

        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        public void RefreshStartingBalance()
        {
            _service
                .Value
                .AllCollectionInitialized?.ContinueWith(
                    _ => StartingBalance.Value = _service.Value.All?.Sum(account => account.StartingBalance.Value) ?? 0L);
        }
    }
}
