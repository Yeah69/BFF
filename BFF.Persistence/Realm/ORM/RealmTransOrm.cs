using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Persistence.Models;
using MoreLinq;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmTransOrm : ITransOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmTransOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public IEnumerable<ITransRealm> GetPageFromSpecificAccount(int offset, int pageSize, IAccountRealm account)
        {
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .Where(t =>
                    (t.Type == TransType.Transaction || t.Type == TransType.ParentTransaction) && t.Account.Name == account.Name
                    || t.Type == TransType.Transfer && (t.ToAccount != null && t.ToAccount.Name == account.Name || t.FromAccount != null && t.FromAccount.Name == account.Name))
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            var collection = query as IRealmCollection<Trans>;
            for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
            {
                yield return collection[i];
            }
        }

        public IEnumerable<ITransRealm> GetPageFromSummaryAccount(int offset, int pageSize)
        {
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            var collection = query as IRealmCollection<Trans>;
            for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
            {
                yield return collection[i];
            }
        }

        public long GetCountFromSpecificAccount(IAccountRealm account)
        {
            return _provideConnection
                .Connection
                .All<Trans>()
                .Count(t =>
                    (t.Type == TransType.Transaction || t.Type == TransType.ParentTransaction) && t.Account.Name == account.Name
                    || t.Type == TransType.Transfer && (t.ToAccount != null && t.ToAccount.Name == account.Name || t.FromAccount != null && t.FromAccount.Name == account.Name));
        }

        public long GetCountFromSummaryAccount()
        {
            return _provideConnection.Connection.All<Trans>().Count();
        }

        public async Task<IEnumerable<ITransRealm>> GetPageFromSpecificAccountAsync(int offset, int pageSize,
            IAccountRealm account)
        {
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .Where(t =>
                    (t.Type == TransType.Transaction || t.Type == TransType.ParentTransaction) &&
                    t.Account.Name == account.Name
                    || t.Type == TransType.Transfer && (t.ToAccount != null && t.ToAccount.Name == account.Name ||
                                                        t.FromAccount != null && t.FromAccount.Name == account.Name))
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            var collection = query as IRealmCollection<Trans>;
            var ret = new List<ITransRealm>();
            for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
            {
                ret.Add(collection[i]);
            }

            return ret;
        }

        public async Task<IEnumerable<ITransRealm>> GetPageFromSummaryAccountAsync(int offset, int pageSize)
        {
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            var collection = query as IRealmCollection<Trans>;
            var ret = new List<ITransRealm>();
            for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
            {
                ret.Add(collection[i]);
            }

            return ret;
        }

        public async Task<IEnumerable<ITransRealm>> GetFromMonthAsync(DateTime month)
        {
            var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .Where(t => t.Date == utcMonth)
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            return query;
        }

        public async Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryRealm category)
        {
            var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .Where(t => t.Date == utcMonth && t.Category != null && t.Category.Id == category.Id)
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            return query;
        }

        public async Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoriesAsync(DateTime month, ICategoryRealm[] categories)
        {
            var categoriesIdSet = categories.Select(c => c.Id).ToHashSet();
            var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
            var query = _provideConnection
                .Connection
                .All<Trans>()
                .Where(t => t.Date == utcMonth && t.Category != null && categoriesIdSet.Contains(t.Category.Id))
                .OrderBy(t => t.Date)
                .ThenByDescending(t => t.Sum);

            return query;
        }

        public async Task<long> GetCountFromSpecificAccountAsync(IAccountRealm account)
        {
            return _provideConnection
                .Connection
                .All<Trans>()
                .Count(t => 
                    (t.Type == TransType.Transaction || t.Type == TransType.ParentTransaction) && t.Account.Name == account.Name
                    || t.Type == TransType.Transfer && (t.ToAccount != null && t.ToAccount.Name == account.Name || t.FromAccount != null && t.FromAccount.Name == account.Name));
        }

        public async Task<long> GetCountFromSummaryAccountAsync()
        {
            return _provideConnection.Connection.All<Trans>().Count();
        }
    }
}
