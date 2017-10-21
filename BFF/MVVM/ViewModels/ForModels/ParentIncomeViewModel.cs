using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IParentIncomeViewModel : IParentTransIncViewModel
    {
        ReadOnlyReactiveCollection<ISubIncomeViewModel> SubIncomes { get; }

        /// <summary>
        /// The SubElements of this ParentElement, which are inserted into the database already.
        /// </summary>
        ReadOnlyReactiveCollection<ISubIncomeViewModel> SubElements { get; }

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        ReadOnlyObservableCollection<ISubIncomeViewModel> NewSubElements { get; }

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
        ReactiveCommand<IAccountViewModel> OpenParentTitView { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentIncome.
    /// </summary>
    public class ParentIncomeViewModel : ParentTransIncViewModel, IParentIncomeViewModel
    {
        private readonly ObservableCollection<ISubIncomeViewModel> _newIncomes;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="parentIncome">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="subIncomeViewModelService">A service for fetching sub-incomes.</param>
        public ParentIncomeViewModel(
            IParentIncome parentIncome,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            IBffOrm orm,
            ISubIncomeViewModelService subIncomeViewModelService) : base(parentIncome, newPayeeViewModelFactory, orm)
        {
            _newIncomes = new ObservableCollection<ISubIncomeViewModel>();
            NewSubElements = new ReadOnlyObservableCollection<ISubIncomeViewModel>(_newIncomes);

            SubIncomes =
                parentIncome.SubIncomes.ToReadOnlyReactiveCollection(subIncomeViewModelService
                    .GetViewModel).AddTo(CompositeDisposable);
            Sum = new ReactiveProperty<long>(SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);

            Sum.DistinctUntilChanged().Subscribe(_ =>
            {
                Account.Value?.RefreshBalance();
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
            }).AddTo(CompositeDisposable);

            SubIncomes.ObserveAddChanged().Concat(SubIncomes.ObserveRemoveChanged())
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))//todo: Write an SQL query for that
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveReplaceChanged()
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveRemoveChanged()
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveResetChanged()
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveElementObservableProperty(sivw => sivw.Sum)
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);

            NewSubElementCommand.Subscribe(_ =>
            {
                var newSubTransactionViewModel = subIncomeViewModelService.Create(parentIncome);
                _newIncomes.Add(newSubTransactionViewModel);
            }).AddTo(CompositeDisposable);

            ApplyCommand.Subscribe(_ =>
            {
                foreach (ISubIncomeViewModel subTransaction in _newIncomes)
                {
                    if (parentIncome.Id > 0L)
                        subTransaction.Insert();
                }
                _newIncomes.Clear();
                OnPropertyChanged(nameof(Sum));
                NotifyRelevantAccountsToRefreshBalance();
            }).AddTo(CompositeDisposable);

            OpenParentTitView.Subscribe(
                avm => Messenger.Default.Send(new ParentTitViewModel(this, "Yeah69", avm))).AddTo(CompositeDisposable);
        }
        
        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return Orm.SubTransactionViewModelService.GetViewModel(subElement as ISubTransaction);
        }

        public ReadOnlyReactiveCollection<ISubIncomeViewModel> SubIncomes { get; }
        public ReadOnlyReactiveCollection<ISubIncomeViewModel> SubElements => SubIncomes;
        public ReadOnlyObservableCollection<ISubIncomeViewModel> NewSubElements { get; }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Account.Value != null && Payee.Value != null && NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        protected override void InitializeDeleteCommand()
        {
            DeleteCommand.Subscribe(_ =>
            {
                var tempList = new List<ISubIncomeViewModel>(SubElements);
                foreach (ISubIncomeViewModel subTransaction in tempList)
                    subTransaction.Delete();
                _newIncomes.Clear();
                Delete();
                NotifyRelevantAccountsToRefreshBalance();
                NotifyRelevantAccountsToRefreshTits();
            });
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
        public ReactiveCommand<IAccountViewModel> OpenParentTitView { get; } = new ReactiveCommand<IAccountViewModel>();

        protected override void OnInsert()
        {
            foreach (var subTransaction in _newIncomes)
            {
                subTransaction.Insert();
            }
            _newIncomes.Clear();
        }
    }
}
