using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Domain;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{

    internal interface ISqliteCategoryBaseRepositoryInternal : ISqliteReadOnlyRepository<ICategoryBase>
    {
    }

    internal class SqliteCategoryBaseRepository : ISqliteCategoryBaseRepositoryInternal
    {
        private readonly ISqliteCategoryRepositoryInternal _categoryRepository;
        private readonly ISqliteIncomeCategoryRepositoryInternal _incomeCategoryRepository;

        public SqliteCategoryBaseRepository(ISqliteCategoryRepositoryInternal categoryRepository, ISqliteIncomeCategoryRepositoryInternal incomeCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
        }

        public async Task<ICategoryBase> FindAsync(long id)
        {
            ICategoryBase ret = (await _incomeCategoryRepository.AllAsync.ConfigureAwait(false)).OfType<ISqlModel>().FirstOrDefault(ic => ic.Id == id) as ICategoryBase; 
            if (ret != null) return ret;
            return (await _categoryRepository.AllAsync.ConfigureAwait(false)).OfType<ISqlModel>().FirstOrDefault(c => c.Id == id) as ICategoryBase;
        }
    }
}
