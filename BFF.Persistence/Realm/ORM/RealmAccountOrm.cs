using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmAccountOrm : IAccountOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmAccountOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task<long?> GetClearedBalanceAsync(IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long? Inner(Realms.Realm realm)
            {
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared
                                && (t.TypeIndex == (int) TransType.Transaction && t.AccountRef == account
                                    || t.TypeIndex == (int) TransType.Transfer && t.ToAccountRef == account))
                    .ToList()
                    .Sum(t => t.Sum);
                var fromTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared
                                && t.TypeIndex == (int)TransType.Transfer 
                                && t.FromAccountRef == account)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.AccountRef == account
                                && t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum + account.StartingBalance;
            }
        }

        public Task<long?> GetClearedBalanceUntilNowAsync(IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long? Inner(Realms.Realm realm)
            {
                var now = DateTimeOffset.UtcNow;
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared 
                                && t.DateOffset <= now
                                && (t.TypeIndex == (int)TransType.Transaction && t.AccountRef == account
                                  || t.TypeIndex == (int)TransType.Transfer && t.ToAccountRef == account))
                    .ToList()
                    .Sum(t => t.Sum);
                var fromTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared 
                                && t.DateOffset <= now
                                && t.TypeIndex == (int)TransType.Transfer && t.FromAccountRef == account)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.AccountRef == account
                                && t.DateOffset <= now
                                && t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum + (account.StartingDate < now ? account.StartingBalance : 0L);
            }
        }

        public Task<long?> GetClearedOverallBalanceAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static long? Inner(Realms.Realm realm)
            {
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared && t.TypeIndex == (int)TransType.Transaction)
                    .ToList()
                    .Sum(st => st.Sum);

                var subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }

                var accountStartingBalanceSum = 0L;
                foreach (var account in realm
                    .All<Account>())
                {
                    accountStartingBalanceSum += account.StartingBalance;
                }

                return transactionsAndToTransfersSum + subTransactionsSum + accountStartingBalanceSum;
            }
        }

        public Task<long?> GetClearedOverallBalanceUntilNowAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static long? Inner(Realms.Realm realm)
            {
                var now = DateTimeOffset.UtcNow;
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => t.Cleared 
                                && t.DateOffset <= now 
                                && t.TypeIndex == (int)TransType.Transaction)
                    .ToList()
                    .Sum(st => st.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.DateOffset <= now
                                && t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }

                var accountStartingBalanceSum = 0L;
                foreach (var account in realm
                    .All<Account>()
                    .Where(a => a.StartingDate < now))
                {
                    accountStartingBalanceSum += account.StartingBalance;
                }
                return transactionsAndToTransfersSum + subTransactionsSum + accountStartingBalanceSum;
            }
        }

        public Task<long?> GetUnclearedBalanceAsync(IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long? Inner(Realms.Realm realm)
            {
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared
                                && (t.TypeIndex == (int)TransType.Transaction && t.AccountRef == account
                                    || t.TypeIndex == (int)TransType.Transfer && t.ToAccountRef == account))
                    .ToList()
                    .Sum(t => t.Sum);
                var fromTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared
                                && t.TypeIndex == (int)TransType.Transfer 
                                && t.FromAccountRef == account)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.AccountRef == account
                                && !t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
            }
        }

        public Task<long?> GetUnclearedBalanceUntilNowAsync(IAccountRealm account)
        {
            return _realmOperations.RunFuncAsync(Inner);

            long? Inner(Realms.Realm realm)
            {
                var now = DateTimeOffset.UtcNow;
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared 
                                && t.DateOffset <= now
                                && (t.TypeIndex == (int)TransType.Transaction && t.AccountRef == account
                                   || t.TypeIndex == (int)TransType.Transfer && t.ToAccountRef == account))
                    .ToList()
                    .Sum(t => t.Sum);
                var fromTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared 
                                && t.DateOffset <= now
                                && t.TypeIndex == (int)TransType.Transfer 
                                && t.FromAccountRef == account)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.AccountRef == account
                                && t.DateOffset <= now
                                && !t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
            }
        }

        public Task<long?> GetUnclearedOverallBalanceAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static long? Inner(Realms.Realm realm)
            {
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared && t.TypeIndex == (int)TransType.Transaction)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && !t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum;
            }
        }

        public Task<long?> GetUnclearedOverallBalanceUntilNowAsync()
        {
            return _realmOperations.RunFuncAsync(Inner);

            static long? Inner(Realms.Realm realm)
            {
                var now = DateTimeOffset.UtcNow;
                var transactionsAndToTransfersSum = realm
                    .All<Trans>()
                    .Where(t => !t.Cleared && t.DateOffset <= now && t.TypeIndex == (int)TransType.Transaction)
                    .ToList()
                    .Sum(t => t.Sum);

                long subTransactionsSum = 0L;
                foreach (var parentTransaction in realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.DateOffset <= now
                                && !t.Cleared))
                {
                    subTransactionsSum += realm
                        .All<SubTransaction>()
                        .Where(st => st.ParentRef == parentTransaction)
                        .ToList()
                        .Sum(st => st.Sum);
                }
                return transactionsAndToTransfersSum + subTransactionsSum;
            }
        }
    }
}
