using System.Linq;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ICategoryBaseRepository : IReadOnlyRepository<ICategoryBase>
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
    }
}
