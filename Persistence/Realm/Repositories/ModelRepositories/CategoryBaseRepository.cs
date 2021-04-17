using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{

    internal interface IRealmCategoryBaseRepositoryInternal : IRealmReadOnlyRepository<ICategoryBase, Models.Persistence.Category>
    {
    }

    internal class RealmCategoryBaseRepository : IRealmCategoryBaseRepositoryInternal
    {
        private readonly IRealmCategoryRepositoryInternal _categoryRepository;
        private readonly IRealmIncomeCategoryRepositoryInternal _incomeCategoryRepository;

        public RealmCategoryBaseRepository(IRealmCategoryRepositoryInternal categoryRepository, IRealmIncomeCategoryRepositoryInternal incomeCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
        }

        public async Task<ICategoryBase> FindAsync(Models.Persistence.Category realmObject)
        {
            return realmObject.IsIncomeRelevant
                ? await _incomeCategoryRepository.FindAsync(realmObject).ConfigureAwait(false)
                : await _categoryRepository.FindAsync(realmObject).ConfigureAwait(false) as ICategoryBase;
        }
    }
}
