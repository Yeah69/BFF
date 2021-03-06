﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

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

        public Task<IEnumerable<SubTransaction>> ReadSubTransactionsOfAsync(Trans parentTransaction)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<SubTransaction> Inner(Realms.Realm realm)
            {
                return parentTransaction.SubTransactions ?? Enumerable.Empty<SubTransaction>();
            }
        }
    }
}
