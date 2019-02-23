using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Models.Realm;
using BFF.Persistence.ORM.Realm.Interfaces;

namespace BFF.Persistence.ORM.Realm
{
    internal class RealmAccountOrm : IAccountOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmAccountOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<long?> GetClearedBalanceAsync(IAccountRealm account)
        {
            var realm = _provideConnection.Connection;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared 
                            && (t.Type == TransType.Transaction && t.Account.Name == account.Name
                            || t.Type == TransType.Transfer && t.ToAccount != null && t.ToAccount.Name == account.Name))
                .Sum(t => t.Sum);
            var fromTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared 
                            && t.Type == TransType.Transfer && t.FromAccount != null && t.FromAccount.Name == account.Name)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => st.Parent.Cleared 
                             && st.Parent.Account.Name == account.Name)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
        }

        public async Task<long?> GetClearedBalanceUntilNowAsync(IAccountRealm account)
        {
            var realm = _provideConnection.Connection;
            var now = DateTime.UtcNow;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared && t.Date <= now 
                            && (t.Type == TransType.Transaction && t.Account.Name == account.Name
                            || t.Type == TransType.Transfer && t.ToAccount != null && t.ToAccount.Name == account.Name))
                .Sum(t => t.Sum);
            var fromTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared && t.Date <= now
                            && t.Type == TransType.Transfer && t.FromAccount != null && t.FromAccount.Name == account.Name)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => st.Parent.Cleared && st.Parent.Date <= now
                             && st.Parent.Account.Name == account.Name)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
        }

        public async Task<long?> GetClearedOverallBalanceAsync()
        {
            var realm = _provideConnection.Connection;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared && t.Type == TransType.Transaction)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => st.Parent.Cleared)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum;
        }

        public async Task<long?> GetClearedOverallBalanceUntilNowAsync()
        {
            var realm = _provideConnection.Connection;
            var now = DateTime.UtcNow;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => t.Cleared && t.Date <= now && t.Type == TransType.Transaction)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => st.Parent.Cleared && st.Parent.Date <= now)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum;
        }

        public async Task<long?> GetUnclearedBalanceAsync(IAccountRealm account)
        {
            var realm = _provideConnection.Connection;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared
                            && (t.Type == TransType.Transaction && t.Account.Name == account.Name
                                || t.Type == TransType.Transfer && t.ToAccount != null && t.ToAccount.Name == account.Name))
                .Sum(t => t.Sum);
            var fromTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared
                            && t.Type == TransType.Transfer && t.FromAccount != null && t.FromAccount.Name == account.Name)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => !st.Parent.Cleared
                             && st.Parent.Account.Name == account.Name)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
        }

        public async Task<long?> GetUnclearedBalanceUntilNowAsync(IAccountRealm account)
        {
            var realm = _provideConnection.Connection;
            var now = DateTime.UtcNow;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared && t.Date <= now
                                      && (t.Type == TransType.Transaction && t.Account.Name == account.Name
                                          || t.Type == TransType.Transfer && t.ToAccount != null && t.ToAccount.Name == account.Name))
                .Sum(t => t.Sum);
            var fromTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared && t.Date <= now
                                      && t.Type == TransType.Transfer && t.FromAccount != null && t.FromAccount.Name == account.Name)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => !st.Parent.Cleared && st.Parent.Date <= now
                                               && st.Parent.Account.Name == account.Name)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum - fromTransfersSum;
        }

        public async Task<long?> GetUnclearedOverallBalanceAsync()
        {
            var realm = _provideConnection.Connection;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared && t.Type == TransType.Transaction)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => !st.Parent.Cleared)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum;
        }

        public async Task<long?> GetUnclearedOverallBalanceUntilNowAsync()
        {
            var realm = _provideConnection.Connection;
            var now = DateTime.UtcNow;
            var transactionsAndToTransfersSum = realm
                .All<Trans>()
                .Where(t => !t.Cleared && t.Date <= now && t.Type == TransType.Transaction)
                .Sum(t => t.Sum);
            var subTransactionsSum = realm
                .All<SubTransaction>()
                .Where(st => !st.Parent.Cleared && st.Parent.Date <= now)
                .Sum(st => st.Sum);
            return transactionsAndToTransfersSum + subTransactionsSum;
        }
    }
}
