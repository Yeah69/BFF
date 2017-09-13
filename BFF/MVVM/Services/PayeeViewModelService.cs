using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IPayeeViewModelService : ICommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>
    {
    }

    public class PayeeViewModelService : CommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>, IPayeeViewModelService
    {
        private readonly IPayeeRepository _repository;
        private readonly IBffOrm _orm;

        public PayeeViewModelService(IPayeeRepository repository, IBffOrm orm) : base(repository)
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
