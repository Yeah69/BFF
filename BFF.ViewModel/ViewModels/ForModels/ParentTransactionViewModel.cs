﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IParentTransactionViewModel : ITransactionBaseViewModel
    {
        ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }

        /// <summary>
        /// The SubTransactions of this ParentElement, which are not inserted into the database yet.
        /// These SubTransactions are in the process of being created and inserted to the database.
        /// </summary>
        ReadOnlyObservableCollection<ISubTransactionViewModel> NewSubTransactions { get; }

        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        IRxRelayCommand NewSubTransactionCommand { get; }

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        IRxRelayCommand ApplyCommand { get; }

        bool IsDateLong { get; }

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        IRxRelayCommand<IAccountViewModel> OpenParentTransactionView { get; }

        IReactiveProperty<long> SumDuringEdit { get; }

        IReadOnlyReactiveProperty<long> MissingSum { get; }

        IReadOnlyReactiveProperty<long> IntermediateSum { get; }

        IReadOnlyReactiveProperty<long> TotalSum { get; }
        IRxRelayCommand NonInsertedConvertToTransaction { get; }
        IRxRelayCommand NonInsertedConvertToTransfer { get; }
        IRxRelayCommand InsertedConvertToTransaction { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    public sealed class ParentTransactionViewModel : TransactionBaseViewModel, IParentTransactionViewModel
    {
        private readonly IBffSettings _bffSettings;
        public ITransDataGridColumnManager TransDataGridColumnManager { get; }
        private readonly SerialDisposable _removeRequestSubscriptions = new SerialDisposable();
        private CompositeDisposable _currentRemoveRequestSubscriptions = new CompositeDisposable();

        private readonly ObservableCollection<ISubTransactionViewModel> _newTransactions;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubTransactions.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }
        
        public ParentTransactionViewModel(
            IParentTransaction parentTransaction,
            INewPayeeViewModel newPayeeViewModel,
            INewFlagViewModel newFlagViewModel,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            IFlagViewModelService flagViewModelService,
            IAccountViewModelService accountViewModelService,
            ITransDataGridColumnManager transDataGridColumnManager,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ILastSetDate lastSetDate,
            ILocalizer localizer,
            IBffSettings bffSettings,
            ITransTransformingManager transTransformingManager,
            IRxSchedulerProvider rxSchedulerProvider,
            ISummaryAccountViewModel summaryAccountViewModel,
            IPayeeViewModelService payeeViewModelService,
            ICreateNewModels createNewModels,
            Func<ISubTransaction, IAccountBaseViewModel, ISubTransactionViewModel> subTransactionViewModelFactory, 
            IAccountBaseViewModel owner) 
            : base(
                parentTransaction,
                newPayeeViewModel,
                newFlagViewModel,
                accountViewModelService,
                payeeViewModelService, 
                lastSetDate,
                localizer,
                rxSchedulerProvider,
                summaryAccountViewModel,
                flagViewModelService,
                owner)
        {
            _bffSettings = bffSettings;
            TransDataGridColumnManager = transDataGridColumnManager;
            _newTransactions = new ObservableCollection<ISubTransactionViewModel>();
            NewSubTransactions = new ReadOnlyObservableCollection<ISubTransactionViewModel>(_newTransactions);

            SubTransactions =
                parentTransaction.SubTransactions.ToReadOnlyReactiveCollection(st => subTransactionViewModelFactory(st, owner)).AddTo(CompositeDisposable);

            Sum = new ReactiveProperty<long>(
                    EmitOnSumRelatedChanges(SubTransactions)
                    .ObserveOn(rxSchedulerProvider.Task)
                    .Select(_ => SubTransactions.Sum(st => st.Sum.Value)), // todo: Write an SQL query for that
                SubTransactions.Sum(st => st.Sum.Value),  // todo: Write an SQL query for that
                ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            SumDuringEdit = new ReactiveProperty<long>(Sum.Value, ReactivePropertyMode.DistinctUntilChanged);

            SumEdit = createSumEdit(SumDuringEdit);

            IntermediateSum =
                EmitOnSumRelatedChanges(NewSubTransactions)
                .Merge(Sum.Select(_ => Unit.Default))
                    .Select(_ => Sum.Value + NewSubTransactions.Sum(ns => ns.Sum.Value))
                    .ToReadOnlyReactivePropertySlim(
                        Sum.Value + NewSubTransactions.Sum(ns => ns.Sum.Value),
                        ReactivePropertyMode.DistinctUntilChanged);

            MissingSum = SumEdit.Sum
                .Merge(IntermediateSum)
                .Select(_ => SumEdit.Sum.Value - IntermediateSum.Value)
                .ToReadOnlyReactivePropertySlim(
                    SumEdit.Sum.Value - IntermediateSum.Value,
                    ReactivePropertyMode.DistinctUntilChanged);

            parentTransaction
                .SubTransactions
                .ObserveElementPropertyChanged()
                .Where(_ => _.EventArgs.PropertyName == nameof(ISubTransaction.Sum) && parentTransaction.IsInserted)
                .Subscribe(_ =>
                {
                    Account?.RefreshBalance();
                    summaryAccountViewModel.RefreshBalance();
                }).AddTo(CompositeDisposable);

            TotalSum = EmitOnSumRelatedChanges(SubTransactions)
                .Merge(EmitOnSumRelatedChanges(NewSubTransactions))
                .Select(_ => SubTransactions.Sum(st => st.Sum.Value) + NewSubTransactions.Sum(st => st.Sum.Value))
                .ToReadOnlyReactivePropertySlim(
                    SubTransactions.Sum(st => st.Sum.Value) + NewSubTransactions.Sum(st => st.Sum.Value),
                    ReactivePropertyMode.DistinctUntilChanged);

            NewSubTransactionCommand = new RxRelayCommand(() =>
            {
                var newSubTransaction = createNewModels.CreateSubTransaction();
                newSubTransaction.Parent = parentTransaction;
                var newSubTransactionViewModel = subTransactionViewModelFactory(newSubTransaction, owner);
                newSubTransactionViewModel.Sum.Value = SumDuringEdit.Value - (Sum.Value + NewSubTransactions.Sum(ns => ns.Sum.Value));
                _newTransactions.Add(newSubTransactionViewModel);
            }).AddTo(CompositeDisposable);

            ApplyCommand = new AsyncRxRelayCommand(async () =>
            {
                if (_newTransactions.All(st => st.IsInsertable()))
                {
                    _currentRemoveRequestSubscriptions = new CompositeDisposable();
                    _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
                    foreach (ISubTransactionViewModel subTransaction in _newTransactions)
                    {
                        if (parentTransaction.IsInserted)
                            await subTransaction.InsertAsync();
                    }
                    _newTransactions.Clear();
                    OnPropertyChanged(nameof(Sum));
                    summaryAccountViewModel.RefreshBalance();
                    Account.RefreshBalance();
                }
            }).AddTo(CompositeDisposable);

            OpenParentTransactionView = new RxRelayCommand<IAccountViewModel>(avm => parentTransactionFlyoutManager.OpenFor(this)).AddTo(CompositeDisposable);
            
            _removeRequestSubscriptions.AddTo(CompositeDisposable);
            _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
            _newTransactions.ObserveAddChanged().Subscribe(t =>
            {
                t.RemoveRequests
                    .Take(1)
                    .Subscribe(_ => _newTransactions.Remove(t))
                    .AddTo(_currentRemoveRequestSubscriptions);
            });

            NonInsertedConvertToTransaction = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToTransactionViewModel(this)))
                .AddTo(CompositeDisposable);

            NonInsertedConvertToTransfer = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToTransferViewModel(this)))
                .AddTo(CompositeDisposable);

            InsertedConvertToTransaction = new AsyncRxRelayCommand(
                    async () =>
                    {
                        var transactionViewModel = transTransformingManager.InsertedToTransactionViewModel(this, SubTransactions.First().Category);
                        await parentTransaction.DeleteAsync();
                        await transactionViewModel.InsertAsync();
                        NotifyRelevantAccountsToRefreshTrans();
                    },
                    SubTransactions
                        .ObservePropertyChanges(nameof(SubTransactions.Count))
                        .Select(_ => SubTransactions.Count >= 1))
                .AddTo(CompositeDisposable);

            IObservable<Unit> EmitOnSumRelatedChanges(ReadOnlyObservableCollection<ISubTransactionViewModel> collection)
                => Observable
                    .Merge(
                        collection
                            .ObserveAddChanged()
                            .Select(_ => Unit.Default),
                        collection
                            .ObserveRemoveChanged()
                            .Select(_ => Unit.Default),
                        collection
                            .ObserveReplaceChanged()
                            .Select(_ => Unit.Default),
                        collection
                            .ObserveRemoveChanged()
                            .Select(_ => Unit.Default),
                        collection
                            .ObserveResetChanged()
                            .Select(_ => Unit.Default),
                        collection
                            .ObserveElementObservableProperty(st => st.Sum)
                            .Select(_ => Unit.Default));
        }

        public ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }
        public ReadOnlyObservableCollection<ISubTransactionViewModel> NewSubTransactions { get; }

        public override ISumEditViewModel SumEdit { get; }

        public override async Task DeleteAsync()
        {
            var tempList = new List<ISubTransactionViewModel>(SubTransactions);
            foreach (ISubTransactionViewModel subTransaction in tempList)
                await subTransaction.DeleteAsync();
            _newTransactions.Clear();
            await base.DeleteAsync();
        }


        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        public IRxRelayCommand NewSubTransactionCommand { get; }

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        public IRxRelayCommand ApplyCommand { get; }

        public bool IsDateLong => _bffSettings.Culture_DefaultDateLong;

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        public IRxRelayCommand<IAccountViewModel> OpenParentTransactionView { get; }

        public IReactiveProperty<long> SumDuringEdit { get; }
        public IReadOnlyReactiveProperty<long> MissingSum { get; }
        public IReadOnlyReactiveProperty<long> IntermediateSum { get; }
        public IReadOnlyReactiveProperty<long> TotalSum { get; }
        public IRxRelayCommand NonInsertedConvertToTransaction { get; }
        public IRxRelayCommand NonInsertedConvertToTransfer { get; }
        public IRxRelayCommand InsertedConvertToTransaction { get; }

        public override async Task InsertAsync()
        {
            await base.InsertAsync();
            foreach (var subTransaction in _newTransactions)
            {
                await subTransaction.InsertAsync();
            }
            _newTransactions.Clear();
        }

        public override void NotifyErrorsIfAny()
        {
            base.NotifyErrorsIfAny();
            foreach (var subTransactionViewModel in NewSubTransactions)
            {
                subTransactionViewModel.NotifyErrorsIfAny();
            }
        }

        public override bool IsInsertable() => base.IsInsertable() && NewSubTransactions.Any() && NewSubTransactions.All(st => st.IsInsertable());
    }
}
