using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
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
        ICommand NewSubElementCommand { get; }

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        ICommand ApplyCommand { get; }

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        ICommand OpenParentTitView { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentIncome.
    /// </summary>
    public class ParentIncomeViewModel : ParentTransIncViewModel, IParentIncomeViewModel
    {
        private readonly IParentIncome _parentIncome;
        private readonly SubIncomeViewModelService _subIncomeViewModelService;

        private readonly ObservableCollection<ISubIncomeViewModel> _newIncomes;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="parentTransInc">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentIncomeViewModel(
            IParentIncome parentTransInc,
            IBffOrm orm,
            SubIncomeViewModelService subIncomeViewModelService) : base(parentTransInc, orm)
        {
            _parentIncome = parentTransInc;
            _subIncomeViewModelService = subIncomeViewModelService;
            _newIncomes = new ObservableCollection<ISubIncomeViewModel>();
            NewSubElements = new ReadOnlyObservableCollection<ISubIncomeViewModel>(_newIncomes);

            SubIncomes =
                _parentIncome.SubIncomes.ToReadOnlyReactiveCollection(subIncomeViewModelService
                    .GetViewModel);
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
            return Account != null && Payee != null && NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        /// <summary>
        /// Deletes the Model from the database and all ist SubElements, which are already in the database.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            var tempList = new List<ISubIncomeViewModel>(SubElements);
            foreach (ISubIncomeViewModel subTransaction in tempList)
                subTransaction.Delete();
            _newIncomes.Clear();
            Delete();
            NotifyRelevantAccountsToRefreshBalance();
            NotifyRelevantAccountsToRefreshTits();
        });

        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        public ICommand NewSubElementCommand => new RelayCommand(obj =>
        {
            var newSubTransactionViewModel = _subIncomeViewModelService.Create(_parentIncome);
            _newIncomes.Add(newSubTransactionViewModel);
        });

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (ISubIncomeViewModel subTransaction in _newIncomes)
            {
                if (_parentIncome.Id > 0L)
                    subTransaction.Insert();
            }
            _newIncomes.Clear();
            OnPropertyChanged(nameof(Sum));
            NotifyRelevantAccountsToRefreshBalance();
        });

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        public ICommand OpenParentTitView => new RelayCommand(param =>
            Messenger.Default.Send(new ParentTitViewModel(this, "Yeah69", param as IAccount)));

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
