using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using JetBrains.Annotations;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmParentalOrm : IParentalOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmParentalOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task<IEnumerable<ISubTransactionRealm>> ReadSubTransactionsOfAsync([NotNull]ITransRealm parentTransaction)
        {
            parentTransaction = parentTransaction ?? throw new ArgumentNullException(nameof(parentTransaction));
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ISubTransactionRealm> Inner(Realms.Realm realm)
            {
                return parentTransaction.SubTransactions;
            }
        }
    }
}
