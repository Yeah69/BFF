using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncViewModel : ITransIncBaseViewModel
    {
        /// <summary>
        /// Each Transaction or Income can be budgeted to a category.
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Category { get; }
    }

    /// <summary>
    /// Base class for ViewModels of Transaction and Income
    /// </summary>
    public abstract class TransIncViewModel : TransIncBaseViewModel, ITransIncViewModel
    {

        private readonly CategoryViewModelService _categoryViewModelService;

        #region Transaction/Income Properties

        /// <summary>
        /// Each Transaction or Income can be budgeted to a category.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Category { get;  }

        /// <summary>
        /// The amount of money of the exchange of the Transaction or Income.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }
        //{
        //    get => _transInc.Sum;
        //    set
        //    {
        //        if(value == _transInc.Sum) return;
        //        _transInc.Sum = value;
        //        OnUpdate();
        //        Messenger.Default.Send(AccountMessage.RefreshBalance, Account);
        //        OnPropertyChanged();
        //    }
        //}

        #endregion

        /// <summary>
        /// Initializes a TransIncViewModel.
        /// </summary>
        /// <param name="transInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        protected TransIncViewModel(
            ITransInc transInc, 
            IBffOrm orm, 
            AccountViewModelService accountViewModelService,
            PayeeViewModelService payeeViewModelService, 
            CategoryViewModelService categoryViewModelService)
            : base(orm, transInc, accountViewModelService, payeeViewModelService)
        {
            _categoryViewModelService = categoryViewModelService;

            Category = transInc.ToReactivePropertyAsSynchronized(
                ti => ti.Category, 
                categoryViewModelService.GetViewModel, 
                categoryViewModelService.GetModel)
                .AddTo(CompositeDisposable);

            Sum = transInc.ToReactivePropertyAsSynchronized(ti => ti.Sum).AddTo(CompositeDisposable);

            Sum.Skip(1)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Account != null && Payee != null && Category != null;
        }

        #region Category Editing

        //todo: Delegate the Editing?

        private string _categoryText;

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string CategoryText
        {
            get => _categoryText;
            set
            {
                _categoryText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public ICategoryViewModel AddingCategoryParent { get; set; }
        
        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            ICategory newCategory = Orm.BffRepository.CategoryRepository.Create();
            newCategory.Name = CategoryText.Trim();
            newCategory.Parent = CommonPropertyProvider.CategoryViewModelService.GetModel(AddingCategoryParent as CategoryViewModel);
            newCategory.Insert();
            OnPropertyChanged(nameof(AllCategories));
            Category.Value = _categoryViewModelService.GetViewModel(newCategory);
        }, obj =>
        {
            return !string.IsNullOrWhiteSpace(CategoryText) && 
            (AddingCategoryParent == null && (CommonPropertyProvider?.ParentCategoryViewModels.All(pcvm => pcvm.Name.Value != CategoryText) ?? false) ||
            AddingCategoryParent != null && AddingCategoryParent.Categories.All(c => c.Name.Value != CategoryText));
        });

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IObservableReadOnlyList<ICategoryViewModel> AllCategories => CommonPropertyProvider?.AllCategoryViewModels;

        #endregion

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void OnUpdate()
        {
            Messenger.Default.Send(SummaryAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        }
    }
}
