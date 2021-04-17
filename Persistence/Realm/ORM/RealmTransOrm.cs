using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;
using MrMeeseeks.Extensions;
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

        public Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(
            int offset, 
            int pageSize,
            Account account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<Trans> Inner(Realms.Realm realm)
            {
                var query = realm
                    .All<Trans>()
                    .Where(t =>
                        (t.TypeIndex == (int)TransType.Transaction || t.TypeIndex == (int)TransType.ParentTransaction) && t.Account == account
                        || t.TypeIndex == (int)TransType.Transfer && (t.ToAccount == account || t.FromAccount == account))
                    .OrderBy(t => t.Date)
                    .ThenByDescending(t => t.Sum);

                var collection = query as IRealmCollection<Trans> ?? throw new NullReferenceException();
                var ret = new List<Trans>();
                for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
                {
                    ret.Add(collection[i]);
                }

                return ret;
            }
        }

        public Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<Trans> Inner(Realms.Realm realm)
            {
                var query = realm
                    .All<Trans>()
                    .OrderBy(t => t.Date)
                    .ThenByDescending(t => t.Sum);

                var collection = query as IRealmCollection<Trans> ?? throw new NullReferenceException();
                var ret = new List<Trans>();
                for (var i = offset; i < offset + pageSize && i < collection.Count; i++)
                {
                    ret.Add(collection[i]);
                }

                return ret;
            }
        }

        public Task<long> GetCountFromSpecificAccountAsync(Account account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long Inner(Realms.Realm realm)
            {
                return realm
                    .All<Trans>()
                    .Count(t =>
                        (t.TypeIndex == (int)TransType.Transaction || t.TypeIndex == (int)TransType.ParentTransaction) && t.Account == account
                        || t.TypeIndex == (int)TransType.Transfer && (t.ToAccount == account || t.FromAccount == account));
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

        public Task<IEnumerable<Trans>> GetFromMonthAsync(DateTime month)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<Trans> Inner(Realms.Realm realm)
            {
                var utcMonth = month.ToMonthIndex();
                var query = realm
                    .All<Trans>()
                    .Where(t => t.MonthIndex == utcMonth)
                    .OrderBy(t => t.Date)
                    .ThenByDescending(t => t.Sum);

                return query;
            }
        }

        public Task<IEnumerable<Trans>> GetFromMonthAndCategoryAsync(
            DateTime month,
            Category category)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<Trans> Inner(Realms.Realm realm)
            {
                var utcMonth = month.ToMonthIndex();
                var query = realm
                    .All<Trans>()
                    .Where(t => t.MonthIndex == utcMonth && t.Category == category)
                    .OrderBy(t => t.Date)
                    .ThenByDescending(t => t.Sum);

                return query;
            }
        }

        public Task<IEnumerable<Trans>> GetFromMonthAndCategoriesAsync(DateTime month, Category[] categories)
        {
            return _realmOperations.RunFuncAsync(Inner);

            IEnumerable<Trans> Inner(Realms.Realm realm)
            {
                var categoriesIdSet = MoreEnumerable.ToHashSet(categories);
                var utcMonth = month.ToMonthIndex();
                var query = realm
                    .All<Trans>()
                    .Where(t => t.MonthIndex == utcMonth)
                    .OrderBy(t => t.Date)
                    .ThenByDescending(t => t.Sum)
                    .ToList()
                    .Where(t => !(t.Category is null) && categoriesIdSet.Contains(t.Category));

                return query;
            }
        }
    }
}
