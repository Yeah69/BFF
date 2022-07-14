using BFF.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCrudOrm<T> : ICrudOrm<T>, IScopeInstance
        where T : class, IPersistenceModelRealm
    {
        private readonly IProvideRealmConnection _provideConnection;
        private readonly IRealmOperations _realmOperations;

        public RealmCrudOrm(
            IProvideRealmConnection provideConnection,
            IRealmOperations realmOperations)
        {
            _provideConnection = provideConnection;
            _realmOperations = realmOperations;
        }

        public async Task<bool> CreateAsync(Func<Realms.Realm, RealmObject> createNewRealmObject)
        {
            await _realmOperations.RunActionAsync(Inner);
            return true;

            void Inner(Realms.Realm realm)
            {
                realm.Write(
                    () => realm.Add(createNewRealmObject(realm), true));
            }
        }

        public Task<IEnumerable<T>> ReadAllAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<T> Inner(Realms.Realm realm)
            {
                switch (true)
                {
                    case true when typeof(T) == typeof(Account):
                        return realm.All<Account>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(Payee):
                        return realm.All<Payee>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(Category):
                        return realm.All<Category>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(Flag):
                        return realm.All<Flag>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(Trans):
                        return realm.All<Trans>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(SubTransaction):
                        return realm.All<SubTransaction>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(BudgetEntry):
                        return realm.All<BudgetEntry>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(DbSetting):
                        return realm.All<DbSetting>().ToList().OfType<T>();
                    default:
                        throw new ArgumentException("Unexpected generic type!", nameof(T));
                }
            }
        }

        public Task UpdateAsync(T model, Action updateRealmObject)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                realm.Write(
                    () =>
                    {
                        updateRealmObject();
                        realm.Add(model as RealmObject, update: true);
                    });
            }
        }

        public Task DeleteAsync(T model)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                switch (model)
                {
                    case Account account:
                        _provideConnection.Backup($"BeforeDeletionOfAccount{account.Name}");
                        break;
                    case Category category:
                        _provideConnection.Backup($"BeforeDeletionOfCategory{category.Name}");
                        break;
                    case Payee payee:
                        _provideConnection.Backup($"BeforeDeletionOfPayee{payee.Name}");
                        break;
                    case Flag flag:
                        _provideConnection.Backup($"BeforeDeletionOfFlag{flag.Name}");
                        break;
                }

                realm.Write(
                    () => realm.Remove(model as RealmObject));
            }
        }
    }
}
