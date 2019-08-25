using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using JetBrains.Annotations;
using MoreLinq;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmTransOrm : ITransOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmTransOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task<IEnumerable<ITransRealm>> GetPageFromSpecificAccountAsync(
            int offset, 
            int pageSize,
            IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ITransRealm> Inner(Realms.Realm realm)
            {
                var query = realm
                    .All<Trans>()
                    .Where(t =>
                        (t.TypeIndex == (int)TransType.Transaction || t.TypeIndex == (int)TransType.ParentTransaction) && t.AccountRef == account
                        || t.TypeIndex == (int)TransType.Transfer && (t.ToAccountRef == account || t.FromAccountRef == account))
                    .OrderBy(t => t.DateOffset)
                    .ThenByDescending(t => t.Sum);

                var collection = query as IRealmCollection<Trans>;
                var ret = new List<ITransRealm>();
                for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
                {
                    ret.Add(collection[i]);
                }

                return ret;
            }
        }

        public Task<IEnumerable<ITransRealm>> GetPageFromSummaryAccountAsync(int offset, int pageSize)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ITransRealm> Inner(Realms.Realm realm)
            {
                var query = realm
                    .All<Trans>()
                    .OrderBy(t => t.DateOffset)
                    .ThenByDescending(t => t.Sum);

                var collection = query as IRealmCollection<Trans>;
                var ret = new List<ITransRealm>();
                for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
                {
                    ret.Add(collection[i]);
                }

                return ret;
            }
        }

        public Task<IEnumerable<ITransRealm>> GetFromMonthAsync(DateTime month)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ITransRealm> Inner(Realms.Realm realm)
            {
                var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
                var query = realm
                    .All<Trans>()
                    .Where(t => t.DateOffset == utcMonth)
                    .OrderBy(t => t.DateOffset)
                    .ThenByDescending(t => t.Sum);

                return query;
            }
        }

        public Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoryAsync(
            DateTime month,
            [NotNull]ICategoryRealm category)
        {
            category = category ?? throw new ArgumentNullException(nameof(category));
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ITransRealm> Inner(Realms.Realm realm)
            {
                var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
                var query = realm
                    .All<Trans>()
                    .Where(t => t.DateOffset == utcMonth && t.CategoryRef == category)
                    .OrderBy(t => t.DateOffset)
                    .ThenByDescending(t => t.Sum);

                return query;
            }
        }

        public Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoriesAsync(DateTime month, ICategoryRealm[] categories)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<ITransRealm> Inner(Realms.Realm realm)
            {
                var categoriesIdSet = categories.ToHashSet();
                var utcMonth = DateTime.SpecifyKind(month, DateTimeKind.Utc);
                var query = realm
                    .All<Trans>()
                    .Where(t => t.DateOffset == utcMonth && categoriesIdSet.Contains(t.CategoryRef))
                    .OrderBy(t => t.DateOffset)
                    .ThenByDescending(t => t.Sum);

                return query;
            }
        }

        public Task<long> GetCountFromSpecificAccountAsync(IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long Inner(Realms.Realm realm)
            {
                return realm
                    .All<Trans>()
                    .Count(t =>
                        (t.TypeIndex == (int)TransType.Transaction || t.TypeIndex == (int)TransType.ParentTransaction) && t.AccountRef == account
                        || t.TypeIndex == (int)TransType.Transfer && (t.ToAccountRef == account || t.FromAccountRef == account));
            }
        }

        public Task<long> GetCountFromSummaryAccountAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            long Inner(Realms.Realm realm)
            {
                return realm.All<Trans>().Count();
            }
        }
    }
}
