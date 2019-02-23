using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Models.Realm;
using BFF.Persistence.ORM.Realm.Interfaces;

namespace BFF.Persistence.ORM.Realm
{
    internal class RealmParentalOrm : IParentalOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmParentalOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }


        public IEnumerable<SubTransaction> ReadSubTransactionsOf(ITransRealm parentTransaction)
        {
            return _provideConnection.Connection.All<SubTransaction>().Where(st => st.Parent.Id == parentTransaction.Id);
        }

        public async Task<IEnumerable<ISubTransactionRealm>> ReadSubTransactionsOfAsync(ITransRealm parentTransaction)
        {
            return _provideConnection.Connection.All<SubTransaction>().Where(st => st.Parent.Id == parentTransaction.Id);
        }
    }
}
