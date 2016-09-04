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
        ObservableCollection<AccountViewModel> AccountViewModels { get; }
        SummaryAccountViewModel SummaryAccountViewModel { get; }
        ObservableCollection<IPayee> Payees { get; }
        ObservableCollection<ICategory> Categories { get; }
        void Add(IAccount account);
        void Add(IPayee payee);
        void Add(ICategory category);
        void Remove(IAccount account);
        void Remove(IPayee payee);
        void Remove(ICategory category);
        IAccount GetAccount(long id);
        IPayee GetPayee(long id);
        ICategory GetCategory(long id);
        AccountViewModel GetAccountViewModel(long id);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        private IBffOrm _orm;

        public ObservableCollection<IAccount> Accounts { get; private set; }

        public ObservableCollection<AccountViewModel> AccountViewModels { get; private set; }

        public SummaryAccountViewModel SummaryAccountViewModel { get; private set; }

        public ObservableCollection<IPayee> Payees { get; private set; }

        public ObservableCollection<ICategory> Categories { get; private set; }

        //todo PayeeViewModels!?

        //todo CategoryViewModels!?


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
        }

        public void Add(ICategory category)
        {
            category.Insert(_orm);
            if (category.Parent == null) //if new category has no parents then append at the end of the list
                Categories.Add(category);
            else //if new category has parent then append as its last child
            {
                int index = category.Parent.Categories.Count - 2;
                index = index == -1 ? Categories.IndexOf(category.Parent) + 1 :
                    Categories.IndexOf(category.Parent.Categories[index]) + 1;
                Categories.Insert(index, category);
            }
        }

        public void Remove(IAccount account)
        {
            account.Delete(_orm);
            Accounts.Remove(account);
        }

        public void Remove(IPayee payee)
        {
            payee.Delete(_orm);
            Payees.Remove(payee);
        }

        public void Remove(ICategory category)
        {
            category.Delete(_orm);
            Categories.Remove(category);
        }

        public IAccount GetAccount(long id) => Accounts.FirstOrDefault(account => account.Id == id);
        public IPayee GetPayee(long id) => Payees.FirstOrDefault(payee => payee.Id == id);
        public ICategory GetCategory(long id) => Categories.FirstOrDefault(category => category.Id == id);

        public AccountViewModel GetAccountViewModel(long id)
            => AccountViewModels.FirstOrDefault(accountVm => accountVm.Id == id);

        private void InitializeAccounts()
        {
            SummaryAccountViewModel = new SummaryAccountViewModel(_orm);
            Accounts = new ObservableCollection<IAccount>(_orm.GetAll<Account>().OrderBy(account => account.Name));
            AccountViewModels = new ObservableCollection<AccountViewModel>(
                Accounts.Select(account => new AccountViewModel(account, _orm)));
            Accounts.CollectionChanged += (sender, args) =>
            {
                switch(args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        int i = args.NewStartingIndex;
                        foreach(var newItem in args.NewItems)
                        {
                            AccountViewModels.Insert(i, new AccountViewModel(newItem as IAccount, _orm));
                            i++;
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        throw new NotImplementedException();
                    case NotifyCollectionChangedAction.Remove:
                        for(int j = 0; j < args.OldItems.Count; j++)
                        {
                            AccountViewModels.RemoveAt(args.OldStartingIndex);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException();
                    case NotifyCollectionChangedAction.Reset:
                        AccountViewModels.Clear();
                        break;
                }
            };
        }

        private void InitializePayees()
        {
            Payees = new ObservableCollection<IPayee>(_orm.GetAll<Payee>().OrderBy(payee => payee.Name));
        }

        private void InitializeCategories()
        {
            Categories = new ObservableCollection<ICategory>();
            Dictionary<long, ICategory> catagoryDictionary = _orm.GetAll<Category>().Cast<ICategory>().ToDictionary(category => category.Id);
            //Now after every Category is loaded the Parent-Child relations are established
            List<ICategory> parentCategories = new List<ICategory>();
            foreach (ICategory category in catagoryDictionary.Values)
            {
                if (category.ParentId != null && category.ParentId > 0)
                {
                    category.Parent = catagoryDictionary[(long)category.ParentId];
                    category.Parent.Categories.Add(category);
                }
                else parentCategories.Add(category);
            }
            foreach (ICategory parentCategory in parentCategories.OrderBy(category => category.Name))
            {
                Categories.Add(parentCategory);
                FillCategoryWithDescandents(parentCategory,  Categories);
            }
        }



        private void FillCategoryWithDescandents(ICategory parentCategory, IList<ICategory> list)
        {
            foreach (ICategory childCategory in parentCategory.Categories.OrderBy(category => category.Name))
            {
                list.Add(childCategory);
                FillCategoryWithDescandents(childCategory, list);
            }
        }
    }
}
