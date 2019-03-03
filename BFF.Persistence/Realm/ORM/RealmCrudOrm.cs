using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Persistence.Models;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCrudOrm<T> : ICrudOrm<T>
        where T : class, IPersistenceModelRealm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmCrudOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<bool> CreateAsync(T model)
        {
            if (model.IsInserted) return true;
            await _provideConnection.Connection.WriteAsync(
                r =>
                {
                    r.Add(model as RealmObject);
                    model.IsInserted = true;
                }).ConfigureAwait(false);
            return true;
        }

        public async Task<T> ReadAsync(T model)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(IAccountRealm):
                    return _provideConnection
                        .Connection
                        .All<Account>()
                        .Where(a => a.Name == (model as IAccountRealm).Name)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(IPayeeRealm):
                    return _provideConnection
                        .Connection
                        .All<Payee>()
                        .Where(p => p.Name == (model as IPayeeRealm).Name)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(ICategoryRealm):
                    return _provideConnection
                        .Connection
                        .All<Category>()
                        .Where(c => c.Id == (model as ICategoryRealm).Id)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(IFlagRealm):
                    return _provideConnection
                        .Connection
                        .All<Flag>()
                        .Where(f => f.Name == (model as IFlagRealm).Name)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(ITransRealm):
                    return _provideConnection
                        .Connection
                        .All<Trans>()
                        .Where(t => t.Id == (model as ITransRealm).Id)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(ISubTransactionRealm):
                    return _provideConnection
                        .Connection
                        .All<SubTransaction>()
                        .Where(st => st.Id == (model as ISubTransactionRealm).Id)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(IBudgetEntryRealm):
                    return _provideConnection
                        .Connection
                        .All<BudgetEntry>()
                        .Where(be => be.Id == (model as IBudgetEntryRealm).Id)
                        .OfType<T>()
                        .First();
                case true when typeof(T) == typeof(IDbSettingRealm):
                    return _provideConnection
                        .Connection
                        .All<DbSetting>()
                        .Where(ds => ds.Id == (model as IDbSettingRealm).Id)
                        .OfType<T>()
                        .First();
                default:
                    throw new ArgumentException("Unexpected generic type!", nameof(T));
            }
        }

        public async Task<IEnumerable<T>> ReadAllAsync()
        {
            switch (true)
            {
                case true when typeof(T) == typeof(IAccountRealm):
                    return _provideConnection.Connection.All<Account>().OfType<T>();
                case true when typeof(T) == typeof(IPayeeRealm):
                    return _provideConnection.Connection.All<Payee>().OfType<T>();
                case true when typeof(T) == typeof(ICategoryRealm):
                    return _provideConnection.Connection.All<Category>().OfType<T>();
                case true when typeof(T) == typeof(IFlagRealm):
                    return _provideConnection.Connection.All<Flag>().OfType<T>();
                case true when typeof(T) == typeof(ITransRealm):
                    return _provideConnection.Connection.All<Trans>().OfType<T>();
                case true when typeof(T) == typeof(ISubTransactionRealm):
                    return _provideConnection.Connection.All<SubTransaction>().OfType<T>();
                case true when typeof(T) == typeof(IBudgetEntryRealm):
                    return _provideConnection.Connection.All<BudgetEntry>().OfType<T>();
                case true when typeof(T) == typeof(IDbSettingRealm):
                    return _provideConnection.Connection.All<DbSetting>().OfType<T>();
                default:
                    throw new ArgumentException("Unexpected generic type!", nameof(T));
            }
        }

        public async Task UpdateAsync(T model)
        {
            if (model.IsInserted.Not()) return;
            await _provideConnection.Connection.WriteAsync(
                r => r.Add(model as RealmObject, update: true)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(T model)
        {
            if (model.IsInserted.Not()) return;
            var realm = _provideConnection.Connection;
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
            await realm.WriteAsync(
                r => r.Remove(model as RealmObject)).ConfigureAwait(false);
        }
    }
}
