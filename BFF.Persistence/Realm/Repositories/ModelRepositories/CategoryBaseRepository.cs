using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{

    internal interface IRealmCategoryBaseRepositoryInternal : IRealmReadOnlyRepository<ICategoryBase, ICategoryRealm>
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

        public async Task<ICategoryBase> FindAsync(ICategoryRealm realmObject)
        {
            return realmObject.IsIncomeRelevant
                ? await _incomeCategoryRepository.FindAsync(realmObject).ConfigureAwait(false)
                : await _categoryRepository.FindAsync(realmObject).ConfigureAwait(false) as ICategoryBase;
        }
    }
}
