using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public class AccountViewModelService : ViewModelServiceBase<Account, IAccountViewModel>
    {
        private readonly IBffOrm _orm;

        private readonly AccountRepository _repository;

        public ISummaryAccountViewModel SummaryAccountViewModel { get; } 

        public AccountViewModelService(AccountRepository repository, IBffOrm orm) : base(repository, true)
        {
            _orm = orm;
            _repository = repository;

            SummaryAccountViewModel = new SummaryAccountViewModel(orm, new SummaryAccount(repository), repository);
            
            All = new TransformingObservableReadOnlyList<Account ,IAccountViewModel>(
                new WrappingObservableReadOnlyList<Account>(repository.All),
                AddToDictionaries);
        }

        protected override IAccountViewModel Create(Account model) => new AccountViewModel(model, _orm, SummaryAccountViewModel);
        public override IAccountViewModel GetNewNonInsertedViewModel() 
            => new AccountViewModel(_repository.Create(), _orm, SummaryAccountViewModel);
    }
}