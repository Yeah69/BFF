using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.DB
{
    public class CommonPropertyProvider
    {
        private IBffOrm _orm;

        public ObservableCollection<Account> Accounts { get; private set; }

        public ObservableCollection<AccountViewModel> AccountViewModels { get; private set; }

        public AllAccountsViewModel AllAccountsViewModel { get; private set; }

        public ObservableCollection<Payee> Payees { get; private set; }

        //todo PayeeViewModels!?

        //todo Categories

        //todo CategoryViewModels!?


        public CommonPropertyProvider(IBffOrm orm)
        {
            _orm = orm;

            InitializeAccounts();

            Payees = new ObservableCollection<Payee>(_orm.GetAll<Payee>());
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

        public Account GetAccount(long id) => Accounts.FirstOrDefault(account => account.Id == id);
        public Payee GetPayee(long id) => Payees.FirstOrDefault(payee => payee.Id == id);

        public AccountViewModel GetAccountViewModel(long id)
            => AccountViewModels.FirstOrDefault(accountVm => accountVm.Id == id);

        private void InitializeAccounts()
        {
            AllAccountsViewModel = new AllAccountsViewModel(_orm);
            Accounts = new ObservableCollection<Account>(_orm.GetAll<Account>());
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
    }
}
