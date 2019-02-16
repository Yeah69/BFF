using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Models.Structure;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ICategoryBaseRepository : IMergingRepository<ICategoryBase>
    {
    }

    internal interface ICategoryBaseRepositoryInternal : ICategoryBaseRepository, IReadOnlyRepository<ICategoryBase>
    {
    }

    internal class CategoryBaseRepository : ICategoryBaseRepositoryInternal
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IIncomeCategoryRepository _incomeCategoryRepository;

        public CategoryBaseRepository(ICategoryRepository categoryRepository, IIncomeCategoryRepository incomeCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
        }

        public Task<ICategoryBase> FindAsync(long id)
        {
            ICategoryBase ret = _incomeCategoryRepository.All.FirstOrDefault(ic => ic.Id == id); 
            if (ret != null) return Task.FromResult(ret);
            return Task.FromResult<ICategoryBase>(_categoryRepository.All.FirstOrDefault(c => c.Id == id));
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
