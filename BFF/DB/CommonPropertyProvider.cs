using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<ICategoryViewModel> AllCategoryViewModels { get; }
        ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get;  }
        ObservableCollection<IPayeeViewModel> AllPayeeViewModels { get; }
        void Add(IAccount account);
        void Add(IPayee payee);
        void Add(ICategory category);
        void Remove(IAccount account);
        void Remove(IPayee payee);
        void Remove(ICategory category);
        IAccountViewModel GetAccountViewModel(long id);
        ICategoryViewModel GetCategoryViewModel(long id);
        IPayeeViewModel GetPayeeViewModel(long id);
        bool IsValidToInsert(ICategoryViewModel categoryViewModel);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        private IBffOrm _orm;
        private readonly BffRepository _bffRepository;

        public ObservableCollection<IAccount> Accounts { get;  }

        public ObservableCollection<IAccountViewModel> AllAccountViewModels { get; }

        public ISummaryAccountViewModel SummaryAccountViewModel { get; }

        public ObservableCollection<IPayee> Payees { get; }

        public ObservableCollection<ICategory> Categories { get; }

        public ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get; private set; }

        public ObservableCollection<ICategoryViewModel> AllCategoryViewModels { get; }

        public ObservableCollection<IPayeeViewModel> AllPayeeViewModels { get; }


        public CommonPropertyProvider(IBffOrm orm, BffRepository bffRepository)
        {
            _orm = orm;
            _bffRepository = bffRepository;
            
            SummaryAccountViewModel = new SummaryAccountViewModel(orm, new SummaryAccount());
            
            Accounts = new ObservableCollection<IAccount>(_bffRepository.AccountRepository.FindAll());
            Categories = new ObservableCollection<ICategory>(_bffRepository.CategoryRepository.FindAll());
            Payees = new ObservableCollection<IPayee>(_bffRepository.PayeeRepository.FindAll());
            
            AllAccountViewModels = new ObservableCollection<IAccountViewModel>(Accounts.Select(a => new AccountViewModel(a, orm)));
            AllPayeeViewModels = new ObservableCollection<IPayeeViewModel>(Payees.Select(p => new PayeeViewModel(p, orm)));
            AllCategoryViewModels = new ObservableCollection<ICategoryViewModel>();
        }

        public void InitializeCategoryViewModels()
        {
            ParentCategoryViewModels = new ObservableCollection<ICategoryViewModel>(
                Categories.Where(c => c.ParentId == null)
                          .OrderBy(c => c.Name)
                          .Select(c => new CategoryViewModel(c, _orm)));
            Stack<ICategoryViewModel> stack = new Stack<ICategoryViewModel>(ParentCategoryViewModels.Reverse());
            while(stack.Count > 0)
            {
                ICategoryViewModel categoryViewModel = stack.Pop();
                AllCategoryViewModels.Add(categoryViewModel);
                categoryViewModel.Categories = new ObservableCollection<ICategoryViewModel>(
                    Categories.Where(c => c.ParentId == categoryViewModel.Id)
                              .OrderBy(c => c.Name)
                              .Select(c => new CategoryViewModel(c, _orm)));
                foreach(ICategoryViewModel child in categoryViewModel.Categories.Reverse())
                {
                    child.Parent = categoryViewModel;
                    stack.Push(child);
                }
            }
        }

        public void Add(IAccount account)
        {
            _bffRepository.AccountRepository.Add(account as Account);
            Accounts.Add(account);
            AllAccountViewModels.Add(new AccountViewModel(account, _orm));
        }

        public void Add(IPayee payee)
        {
            _bffRepository.PayeeRepository.Add(payee as Payee);
            Payees.Add(payee);
            AllPayeeViewModels.Add(new PayeeViewModel(payee, _orm));
        }

        public void Add(ICategory category)
        {
            _bffRepository.CategoryRepository.Add(category as Category);
        }

        public void Remove(IAccount account)
        {
            _bffRepository.AccountRepository.Delete(account as Account);
            Accounts.Remove(account);
            AllAccountViewModels.Remove(GetAccountViewModel(account.Id));
        }

        public void Remove(IPayee payee)
        {
            _bffRepository.PayeeRepository.Delete(payee as Payee);
            Payees.Remove(payee);
            AllPayeeViewModels.Remove(GetPayeeViewModel(payee.Id));
        }

        public void Remove(ICategory category)
        {
            _bffRepository.CategoryRepository.Delete(category as Category);
        }

        public IAccountViewModel GetAccountViewModel(long id)
            => AllAccountViewModels.Single(avm => avm.Id  == id);

        public ICategoryViewModel GetCategoryViewModel(long id)
            => AllCategoryViewModels.Single(cvm => cvm.Id  == id);

        public IPayeeViewModel GetPayeeViewModel(long id)
            => AllPayeeViewModels.Single(pvm => pvm.Id  == id);

        public bool IsValidToInsert(ICategoryViewModel categoryViewModel) => 
            categoryViewModel.Parent == null && ParentCategoryViewModels.All(cvm => cvm.Name != categoryViewModel.Name) ||
            categoryViewModel.Parent.Categories.All(cvm => cvm.Name != categoryViewModel.Name);
    }
}
