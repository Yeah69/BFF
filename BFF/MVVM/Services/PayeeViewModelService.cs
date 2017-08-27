using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class PayeeViewModelService : CommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>
    {
        private readonly PayeeRepository _repository;
        private readonly IBffOrm _orm;

        public PayeeViewModelService(PayeeRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override IPayeeViewModel Create(IPayee model)
            => new PayeeViewModel(model, _orm);
        public override IPayeeViewModel GetNewNonInsertedViewModel()
            => new PayeeViewModel(_repository.Create(), _orm);
    }
}
