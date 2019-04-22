using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.ORM
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
