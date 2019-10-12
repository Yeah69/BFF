using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCategoryOrm : ICategoryOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmCategoryOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task<IEnumerable<Category>> ReadCategoriesAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static IEnumerable<Category> Inner(Realms.Realm realm)
            {
                return realm.All<Category>().Where(c => !c.IsIncomeRelevant);
            }
        }

        public Task<IEnumerable<Category>> ReadIncomeCategoriesAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static IEnumerable<Category> Inner(Realms.Realm realm)
            {
                return realm.All<Category>().Where(c => c.IsIncomeRelevant);
            }
        }
    }
}
