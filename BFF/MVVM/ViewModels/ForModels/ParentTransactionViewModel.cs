using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IParentTransactionViewModel : ITransactionBaseViewModel
    {
        ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }

        /// <summary>
        /// The SubElements of this ParentElement, which are inserted into the database already.
        /// </summary>
        ReadOnlyReactiveCollection<ISubTransactionViewModel> SubElements { get; }

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        ReadOnlyObservableCollection<ISubTransactionViewModel> NewSubElements { get; }

        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        ReactiveCommand NewSubElementCommand { get; }

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        ReactiveCommand ApplyCommand { get; }

        bool IsDateLong { get; }

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        ReactiveCommand<IAccountViewModel> OpenParentTransactionView { get; }

        //IReactiveProperty<long> SumDuringEdit { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    public sealed class ParentTransactionViewModel : TransactionBaseViewModel, IParentTransactionViewModel
    {
        private readonly SerialDisposable _removeRequestSubscriptions = new SerialDisposable();
        private CompositeDisposable _currentRemoveRequestSubscriptions = new CompositeDisposable();

        private readonly ObservableCollection<ISubTransactionViewModel> _newTransactions;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }
        
        public ParentTransactionViewModel(
            IParentTransaction parentTransaction,
            INewPayeeViewModel newPayeeViewModel,
            INewFlagViewModel newFlagViewModel,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            ISubTransactionViewModelService subTransactionViewModelService, 
            IFlagViewModelService flagViewModelService,
            IAccountViewModelService accountViewModelService,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider schedulerProvider,
            IPayeeViewModelService payeeViewModelService) 
            : base(
                parentTransaction,
                newPayeeViewModel,
                newFlagViewModel,
                accountViewModelService,
                payeeViewModelService, 
                lastSetDate,
                schedulerProvider,
                flagViewModelService)
        {
            _newTransactions = new ObservableCollection<ISubTransactionViewModel>();
            NewSubElements = new ReadOnlyObservableCollection<ISubTransactionViewModel>(_newTransactions);

            SubTransactions =
                parentTransaction.SubTransactions.ToReadOnlyReactiveCollection(subTransactionViewModelService
                    .GetViewModel).AddTo(CompositeDisposable);

            Sum = new ReactiveProperty<long>(SubTransactions.Sum(st => st.Sum.Value), ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            
            Sum.Where(_ => parentTransaction.Id != -1L)
                .Subscribe(_ =>
                {
                    Account.Value?.RefreshBalance();
                    Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                }).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            SubTransactions
                .ObserveAddChanged()
                .Concat(SubTransactions.ObserveRemoveChanged())
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(st => st.Sum.Value))//todo: Write an SQL query for that
                .AddTo(CompositeDisposable);
            SubTransactions
                .ObserveReplaceChanged()
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(st => st.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions
                .ObserveRemoveChanged()
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(st => st.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions
                .ObserveResetChanged()
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(st => st.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions
                .ObserveElementObservableProperty(st => st.Sum)
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(st => st.Sum.Value))
                .AddTo(CompositeDisposable);

            NewSubElementCommand.Subscribe(_ =>
            {
                var newSubTransactionViewModel = subTransactionViewModelService.Create(parentTransaction);
                _newTransactions.Add(newSubTransactionViewModel);
            }).AddTo(CompositeDisposable);

            ApplyCommand.Subscribe(_ =>
            {
                if (_newTransactions.All(st => st.IsInsertable()))
                {
                    _currentRemoveRequestSubscriptions = new CompositeDisposable();
                    _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
                    foreach (ISubTransactionViewModel subTransaction in _newTransactions)
                    {
                        if (parentTransaction.Id > 0L)
                            subTransaction.Insert();
                    }
                    _newTransactions.Clear();
                    OnPropertyChanged(nameof(Sum));
                    Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                    Account.Value.RefreshBalance();
                }
                
            }).AddTo(CompositeDisposable);
            
            OpenParentTransactionView
                .Subscribe(avm => parentTransactionFlyoutManager.OpenFor(this))
                .AddTo(CompositeDisposable);

            _removeRequestSubscriptions.AddTo(CompositeDisposable);
            _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
            _newTransactions.ObserveAddChanged().Subscribe(t =>
            {
                t.RemoveRequests
                    .Take(1)
                    .Subscribe(_ => _newTransactions.Remove(t))
                    .AddTo(_currentRemoveRequestSubscriptions);
            });
        }

        public ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }
        public ReadOnlyReactiveCollection<ISubTransactionViewModel> SubElements => SubTransactions;
        public ReadOnlyObservableCollection<ISubTransactionViewModel> NewSubElements { get; }

        public override ISumEditViewModel SumEdit { get; }

        public override void Delete()
        {
            var tempList = new List<ISubTransactionViewModel>(SubElements);
            foreach (ISubTransactionViewModel subTransaction in tempList)
                subTransaction.Delete();
            _newTransactions.Clear();
            base.Delete();
        }


        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        public ReactiveCommand NewSubElementCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        public ReactiveCommand ApplyCommand { get; } = new ReactiveCommand();

        public bool IsDateLong => Settings.Default.Culture_DefaultDateLong;

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        public ReactiveCommand<IAccountViewModel> OpenParentTransactionView { get; } = new ReactiveCommand<IAccountViewModel>();

        protected override void OnInsert()
        {
            foreach (var subTransaction in _newTransactions)
            {
                subTransaction.Insert();
            }
            _newTransactions.Clear();
        }

        public override bool IsInsertable() => base.IsInsertable() && NewSubElements.Any() && NewSubElements.All(st => st.IsInsertable());
    }
}
