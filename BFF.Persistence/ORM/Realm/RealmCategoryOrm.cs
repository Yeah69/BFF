using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Persistence.Models.Realm;
using BFF.Persistence.ORM.Realm.Interfaces;

namespace BFF.Persistence.ORM.Realm
{
    internal class RealmCategoryOrm : ICategoryOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmCategoryOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<IEnumerable<ICategoryRealm>> ReadCategoriesAsync()
        {
            return _provideConnection.Connection.All<Category>().Where(c => c.IsIncomeRelevant.Not());
        }

        public async Task<IEnumerable<ICategoryRealm>> ReadIncomeCategoriesAsync()
        {
            return _provideConnection.Connection.All<Category>().Where(c => c.IsIncomeRelevant);
        }
    }
}
