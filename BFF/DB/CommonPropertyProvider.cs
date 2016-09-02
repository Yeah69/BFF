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
        ObservableCollection<Account> Accounts { get; }
        ObservableCollection<AccountViewModel> AccountViewModels { get; }
        SummaryAccountViewModel SummaryAccountViewModel { get; }
        ObservableCollection<Payee> Payees { get; }
        ObservableCollection<Category> Categories { get; }
        void Add(Account account);
        void Add(Payee payee);
        void Add(Category category);
        void Remove(Account account);
        void Remove(Payee payee);
        void Remove(Category category);
        Account GetAccount(long id);
        Payee GetPayee(long id);
        Category GetCategory(long id);
        AccountViewModel GetAccountViewModel(long id);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        private IBffOrm _orm;

        public ObservableCollection<Account> Accounts { get; private set; }

        public ObservableCollection<AccountViewModel> AccountViewModels { get; private set; }

        public SummaryAccountViewModel SummaryAccountViewModel { get; private set; }

        public ObservableCollection<Payee> Payees { get; private set; }

        public ObservableCollection<Category> Categories { get; private set; }

        //todo PayeeViewModels!?

        //todo CategoryViewModels!?


        public CommonPropertyProvider(IBffOrm orm)
        {
            _orm = orm;

            InitializeAccounts();
            InitializePayees();
            InitializeCategories();
        }

        public void Add(Account account)
        {
            _orm.Insert(account);
            Accounts.Add(account);
        }

        public void Add(Payee payee)
        {
            _orm.Insert(payee);
            Payees.Add(payee);
        }

        public void Add(Category category)
        {
            _orm.Insert(category);
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

        public void Remove(Account account)
        {
            _orm.Delete(account);
            Accounts.Remove(account);
        }

        public void Remove(Payee payee)
        {
            _orm.Delete(payee);
            Payees.Remove(payee);
        }

        public void Remove(Category category)
        {
            _orm.Delete(category);
            Categories.Remove(category);
        }

        public Account GetAccount(long id) => Accounts.FirstOrDefault(account => account.Id == id);
        public Payee GetPayee(long id) => Payees.FirstOrDefault(payee => payee.Id == id);
        public Category GetCategory(long id) => Categories.FirstOrDefault(category => category.Id == id);

        public AccountViewModel GetAccountViewModel(long id)
            => AccountViewModels.FirstOrDefault(accountVm => accountVm.Id == id);

        private void InitializeAccounts()
        {
            SummaryAccountViewModel = new SummaryAccountViewModel(_orm);
            Accounts = new ObservableCollection<Account>(_orm.GetAll<Account>().OrderBy(account => account.Name));
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
                            AccountViewModels.Insert(i, new AccountViewModel(newItem as Account, _orm));
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
            Payees = new ObservableCollection<Payee>(_orm.GetAll<Payee>().OrderBy(payee => payee.Name));
        }

        private void InitializeCategories()
        {
            Categories = new ObservableCollection<Category>();
            Dictionary<long, Category> catagoryDictionary = _orm.GetAll<Category>().ToDictionary(category => category.Id);
            //Now after every Category is loaded the Parent-Child relations are established
            List<Category> parentCategories = new List<Category>();
            foreach (Category category in catagoryDictionary.Values)
            {
                if (category.ParentId != null && category.ParentId > 0)
                {
                    category.Parent = catagoryDictionary[(long)category.ParentId];
                    category.Parent.Categories.Add(category);
                }
                else parentCategories.Add(category);
            }
            foreach (Category parentCategory in parentCategories.OrderBy(category => category.Name))
            {
                Categories.Add(parentCategory);
                FillCategoryWithDescandents(parentCategory,  Categories);
            }
        }



        private void FillCategoryWithDescandents(Category parentCategory, IList<Category> list)
        {
            foreach (Category childCategory in parentCategory.Categories.OrderBy(category => category.Name))
            {
                list.Add(childCategory);
                FillCategoryWithDescandents(childCategory, list);
            }
        }
    }
}
