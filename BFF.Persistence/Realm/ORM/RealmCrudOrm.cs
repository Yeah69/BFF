using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCrudOrm<T> : ICrudOrm<T>
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

        public async Task<bool> CreateAsync(T model)
        {
            await _realmOperations.RunActionAsync(Inner);
            return true;

            void Inner(Realms.Realm realm)
            {
                realm.Write(
                    () =>
                    {
                        realm.Add(model as RealmObject);
                    });
            }
        }

        public Task<IEnumerable<T>> ReadAllAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<T> Inner(Realms.Realm realm)
            {
                switch (true)
                {
                    case true when typeof(T) == typeof(IAccountRealm):
                        return realm.All<Account>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(IPayeeRealm):
                        return realm.All<Payee>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(ICategoryRealm):
                        return realm.All<Category>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(IFlagRealm):
                        return realm.All<Flag>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(ITransRealm):
                        return realm.All<Trans>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(ISubTransactionRealm):
                        return realm.All<SubTransaction>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(IBudgetEntryRealm):
                        return realm.All<BudgetEntry>().ToList().OfType<T>();
                    case true when typeof(T) == typeof(IDbSettingRealm):
                        return realm.All<DbSetting>().ToList().OfType<T>();
                    default:
                        throw new ArgumentException("Unexpected generic type!", nameof(T));
                }
            }
        }

        public Task UpdateAsync(T model)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                realm.Write(
                    () => realm.Add(model as RealmObject, update: true));
            }
        }

        public Task DeleteAsync(T model)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {
                switch (model)
                {
                    case IAccountRealm account:
                        _provideConnection.Backup($"BeforeDeletionOfAccount{account.Name}");
                        break;
                    case ICategoryRealm category:
                        _provideConnection.Backup($"BeforeDeletionOfCategory{category.Name}");
                        break;
                    case IPayeeRealm payee:
                        _provideConnection.Backup($"BeforeDeletionOfPayee{payee.Name}");
                        break;
                    case IFlagRealm flag:
                        _provideConnection.Backup($"BeforeDeletionOfFlag{flag.Name}");
                        break;
                }

                realm.Write(
                    () => realm.Remove(model as RealmObject));
            }
        }
    }
}
