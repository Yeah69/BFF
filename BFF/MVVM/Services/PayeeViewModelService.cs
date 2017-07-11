using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    class PayeeViewModelService : ViewModelServiceBase<Payee, PayeeViewModel>
    {
        private readonly PayeeRepository _repository;
        private readonly IBffOrm _orm;

        public PayeeViewModelService(PayeeRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override PayeeViewModel Create(Payee model)
            => new PayeeViewModel(model, _orm);
        public override PayeeViewModel GetNewNonInsertedViewModel()
            => new PayeeViewModel(_repository.Create(), _orm);
    }
}
