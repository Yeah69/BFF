using System.Linq;
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

        public ICategoryBase Find(long id)
        {
            ICategoryBase ret = _categoryRepository.All.FirstOrDefault(c => c.Id == id);
            if (ret != null) return ret;
            return _incomeCategoryRepository.All.FirstOrDefault(ic => ic.Id == id);
        }
    }
}
