using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native;

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

        public ICategoryBase Create()
        {
            return null;
        }

        public ICategoryBase Find(long id, DbConnection connection = null)
        {
            ICategoryBase ret = _categoryRepository.All.FirstOrDefault(c => c.Id == id);
            if (ret != null) return ret;
            return _incomeCategoryRepository.All.FirstOrDefault(ic => ic.Id == id);
        }
    }
}
