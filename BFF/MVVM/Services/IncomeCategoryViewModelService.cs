using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IIncomeCategoryViewModelService : ICommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>
    {
    }

    public class IncomeCategoryViewModelService : CommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>, IIncomeCategoryViewModelService
    {
        private readonly IIncomeCategoryRepository _repository;
        private readonly IBffOrm _orm;

        public IncomeCategoryViewModelService(IIncomeCategoryRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override IIncomeCategoryViewModel Create(IIncomeCategory model)
            => new IncomeCategoryViewModel(model, _orm);
        public override IIncomeCategoryViewModel GetNewNonInsertedViewModel()
            => new IncomeCategoryViewModel(_repository.Create(), _orm);
    }
}
