using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IFlagViewModelService : ICommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>
    {
    }

    public class FlagViewModelService : CommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>, IFlagViewModelService
    {
        private readonly IFlagRepository _repository;
        private readonly IBffOrm _orm;

        public FlagViewModelService(IFlagRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override IFlagViewModel Create(IFlag model)
            => new FlagViewModel(model, _orm, this);
        public override IFlagViewModel GetNewNonInsertedViewModel()
            => new FlagViewModel(_repository.Create(), _orm, this);
    }
}
