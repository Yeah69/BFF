using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ICategoryBaseRepository
    {
    }

    internal interface ICategoryBaseRepositoryInternal : IMergingRepository<ICategoryBase>, ICategoryBaseRepository, IReadOnlyRepository<ICategoryBase>
    {
    }

    internal class CategoryBaseRepository : ICategoryBaseRepositoryInternal
    {
        private readonly ICategoryRepositoryInternal _categoryRepository;
        private readonly IIncomeCategoryRepositoryInternal _incomeCategoryRepository;

        public CategoryBaseRepository(ICategoryRepositoryInternal categoryRepository, IIncomeCategoryRepositoryInternal incomeCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
        }

        public Task<ICategoryBase> FindAsync(long id)
        {
            ICategoryBase ret = _incomeCategoryRepository.All.OfType<IDataModelInternal<ICategorySql>>().FirstOrDefault(ic => ic.BackingPersistenceModel.Id == id) as ICategoryBase; 
            if (ret != null) return Task.FromResult(ret);
            return Task.FromResult(_categoryRepository.All.OfType<IDataModelInternal<ICategorySql>>().FirstOrDefault(c => c.BackingPersistenceModel.Id == id) as ICategoryBase);
        }

        public Task MergeAsync(ICategoryBase from, ICategoryBase to)
        {
            if (from is ICategory catFrom && to is ICategory catTo)
                return _categoryRepository.MergeAsync(catFrom, catTo);
            else if (from is IIncomeCategory incFrom && to is IIncomeCategory incTo)
                return _incomeCategoryRepository.MergeAsync(incFrom, incTo);
            return Task.CompletedTask;
        }
    }
}
