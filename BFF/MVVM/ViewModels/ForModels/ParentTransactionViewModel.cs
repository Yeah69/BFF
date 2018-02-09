using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
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

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        ReactiveCommand<IAccountViewModel> OpenParentTransactionView { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    public sealed class ParentTransactionViewModel : TransactionBaseViewModel, IParentTransactionViewModel
    {
        private readonly ObservableCollection<ISubTransactionViewModel> _newTransactions;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a ParentTransactionViewModel.
        /// </summary>
        /// <param name="parentTransaction">A ParentTransaction Model.</param>
        /// <param name="newPayeeViewModelFactory">Creates a payee factory.</param>
        /// <param name="subTransactionViewModelService">A service for fetching sub-transactions.</param>
        /// <param name="flagViewModelService">Fetches flags.</param>
        /// <param name="accountViewModelService">Fetches accounts.</param>
        /// <param name="createSumEdit">Creates sum editing viewmodel.</param>
        /// <param name="payeeViewModelService">Fetches payees.</param>
        public ParentTransactionViewModel(
            IParentTransaction parentTransaction,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            ISubTransactionViewModelService subTransactionViewModelService, 
            IFlagViewModelService flagViewModelService,
            IAccountViewModelService accountViewModelService,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IPayeeViewModelService payeeViewModelService) 
            : base(
                parentTransaction,
                newPayeeViewModelFactory,
                accountViewModelService,
                payeeViewModelService,
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
                foreach (ISubTransactionViewModel subTransaction in _newTransactions)
                {
                    if (parentTransaction.Id > 0L)
                        subTransaction.Insert();
                }
                _newTransactions.Clear();
                OnPropertyChanged(nameof(Sum));
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                Account.Value.RefreshBalance();
            }).AddTo(CompositeDisposable);

            OpenParentTransactionView.Subscribe(avm =>
                Messenger.Default.Send(new ParentTitViewModel(this, "Yeah69", avm))).AddTo(CompositeDisposable);
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
    }
}
