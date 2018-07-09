using System.Linq;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ICategoryBaseRepository : IReadOnlyRepository<ICategoryBase>, IMergingRepository<ICategoryBase>
    {
    }

    public class CategoryBaseRepository : ICategoryBaseRepository
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
