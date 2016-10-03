using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.DB
{
    public interface ICommonPropertyProvider {
        ObservableCollection<IAccount> Accounts { get; }
        ObservableCollection<IAccountViewModel> AllAccountViewModels { get; }
        ISummaryAccountViewModel SummaryAccountViewModel { get; }
        ObservableCollection<IPayee> Payees { get; }
        ObservableCollection<ICategory> Categories { get; }
        IEnumerable<ICategoryViewModel> AllCategoryViewModels { get; }
        ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get;  }
        ObservableCollection<IPayeeViewModel> AllPayeeViewModels { get; }
        void Add(IAccount account);
        void Add(IPayee payee);
        void Add(ICategory category);
        void Remove(IAccount account);
        void Remove(IPayee payee);
        void Remove(ICategory category);
        IAccount GetAccount(long id);
        IPayee GetPayee(long id);
        ICategory GetCategory(long id);
        IAccountViewModel GetAccountViewModel(long id);
        ICategoryViewModel GetCategoryViewModel(long id);
        IEnumerable<ICategoryViewModel> GetCategoryViewModelChildren(long parentId);
        IPayeeViewModel GetPayeeViewModel(long id);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        private IBffOrm _orm;

        public ObservableCollection<IAccount> Accounts { get; private set; }

        public ObservableCollection<IAccountViewModel> AllAccountViewModels { get; private set; }

        public ISummaryAccountViewModel SummaryAccountViewModel { get; private set; }

        public ObservableCollection<IPayee> Payees { get; private set; }

        public ObservableCollection<ICategory> Categories { get; private set; }

        public ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get; private set; }

        public IEnumerable<ICategoryViewModel> AllCategoryViewModels
            => ParentCategoryViewModels.SelectMany(pcvm => pcvm as IEnumerable<ICategoryViewModel>);

        public ObservableCollection<IPayeeViewModel> AllPayeeViewModels { get; private set; }


        public CommonPropertyProvider(IBffOrm orm)
        {
            _orm = orm;

            InitializeAccounts();
            InitializePayees();
            InitializeCategories();
        }

        public void Add(IAccount account)
        {
            account.Insert(_orm);
            Accounts.Add(account);
        }

        public void Add(IPayee payee)
        {
            payee.Insert(_orm);
            Payees.Add(payee);
            AllPayeeViewModels.Add(new PayeeViewModel(payee, _orm));
        }

        public void Add(ICategory category)
        {
            category.Insert(_orm);
            Categories.Add(category);
            ICategoryViewModel categoryViewModel = new CategoryViewModel(category, _orm);
            if(category.ParentId != null)
            {
                ICategoryViewModel parent = AllCategoryViewModels.Single(scvm => scvm.Id == category.ParentId);
                parent.Categories.Add(categoryViewModel);
                categoryViewModel.Parent = parent;
            }
            else
                ParentCategoryViewModels.Add(categoryViewModel);
        }

        public void Remove(IAccount account)
        {
            account.Delete(_orm);
            Accounts.Remove(account);
        }

        public void Remove(IPayee payee)
        {
            payee.Delete(_orm);
            IPayeeViewModel viewModel = AllPayeeViewModels.Single(apvm => apvm.Id == payee.Id);
            AllPayeeViewModels.Remove(viewModel);
            Payees.Remove(payee);
        }

        public void Remove(ICategory category)
        {
            category.Delete(_orm);
            ICategoryViewModel viewModel = AllCategoryViewModels.Single(scvm => scvm.Id == category.Id);
            viewModel.Parent?.Categories.Remove(viewModel);
            if(viewModel.Parent == null)
                ParentCategoryViewModels.Remove(viewModel);
            Categories.Remove(category);
        }

        public IAccount GetAccount(long id) => Accounts.FirstOrDefault(account => account.Id == id);
        public IPayee GetPayee(long id) => Payees.FirstOrDefault(payee => payee.Id == id);
        public ICategory GetCategory(long id) => Categories.FirstOrDefault(category => category.Id == id);

        public IAccountViewModel GetAccountViewModel(long id)
            => AllAccountViewModels.FirstOrDefault(accountVm => accountVm.Id == id);

        public IEnumerable<ICategoryViewModel> GetCategoryViewModelChildren(long parentId)
            => ParentCategoryViewModels.Where(cvm => cvm.Parent.Id == parentId);

        public ICategoryViewModel GetCategoryViewModel(long id)
            => AllCategoryViewModels.FirstOrDefault(categoryVm => categoryVm.Id == id);

        public IPayeeViewModel GetPayeeViewModel(long id)
            => AllPayeeViewModels.FirstOrDefault(apvm => apvm.Id == id);

        private void InitializeAccounts()
        {
            //todo: when C#7.0 is released: make this a local function in Constructor
            SummaryAccountViewModel = new SummaryAccountViewModel(_orm);
            Accounts = new ObservableCollection<IAccount>(_orm.GetAll<Account>().OrderBy(account => account.Name));
            AllAccountViewModels = new ObservableCollection<IAccountViewModel>(
                Accounts.Select(account => new AccountViewModel(account, _orm)));
            Accounts.CollectionChanged += (sender, args) =>
            {
                switch(args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        int i = args.NewStartingIndex;
                        foreach(var newItem in args.NewItems)
                        {
                            AllAccountViewModels.Insert(i, new AccountViewModel(newItem as IAccount, _orm));
                            i++;
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        throw new NotImplementedException();
                    case NotifyCollectionChangedAction.Remove:
                        for(int j = 0; j < args.OldItems.Count; j++)
                        {
                            AllAccountViewModels.RemoveAt(args.OldStartingIndex);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException();
                    case NotifyCollectionChangedAction.Reset:
                        AllAccountViewModels.Clear();
                        break;
                }
            };
        }

        private void InitializePayees()
        {
            //todo: when C#7.0 is released: make this a local function in Constructor
            Payees = new ObservableCollection<IPayee>(_orm.GetAll<Payee>().OrderBy(payee => payee.Name));
            AllPayeeViewModels = new ObservableCollection<IPayeeViewModel>(Payees.Select(p => new PayeeViewModel(p, _orm)));
        }

        private void InitializeCategories()
        {
            //todo: when C#7.0 is released: make this a local function in Constructor
            Categories = new ObservableCollection<ICategory>(_orm.GetAll<Category>());
            IEnumerable<ICategory> parentCategories = Categories.Where(c => c.ParentId == 0);
            ParentCategoryViewModels = new ObservableCollection<ICategoryViewModel>(parentCategories.Select(pc => new CategoryViewModel(pc, _orm)));
            foreach (ICategoryViewModel parentCategoryViewModel in ParentCategoryViewModels)
            {
                CreateChildCategoryViewModels(parentCategoryViewModel);
            }
        }

        private void CreateChildCategoryViewModels(ICategoryViewModel parentViewModel)
        {
            IEnumerable<ICategory> children = Categories.Where(c => c.ParentId == parentViewModel.Id);
            parentViewModel.Categories = new ObservableCollection<ICategoryViewModel>(children.Select(c => new CategoryViewModel(c, _orm)));
            foreach(ICategoryViewModel categoryViewModel in parentViewModel.Categories)
            {
                CreateChildCategoryViewModels(categoryViewModel);
            }
        }
    }
}
