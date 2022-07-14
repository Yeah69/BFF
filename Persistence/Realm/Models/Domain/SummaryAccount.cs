using BFF.Core.IoC;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class SummaryAccount : Model.Models.SummaryAccount, IScopeInstance
    {
        private readonly IAccountOrm _accountOrm;
        private readonly IRealmTransRepository _transRepository;

        public SummaryAccount(
            IAccountOrm accountOrm,
            IRealmTransRepository transRepository)
        {
            _accountOrm = accountOrm;
            _transRepository = transRepository;
        }

        public override bool IsInserted => true;
        public override Task InsertAsync()
        {
            return Task.CompletedTask;
        }

        public override Task DeleteAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task UpdateAsync()
        {
            return Task.CompletedTask;
        }

        public override Task<long?> GetClearedBalanceAsync()
        {
            return _accountOrm.GetClearedOverallBalanceAsync();
        }

        public override Task<long?> GetClearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetClearedOverallBalanceUntilNowAsync();
        }

        public override Task<long?> GetUnclearedBalanceAsync()
        {
            return _accountOrm.GetUnclearedOverallBalanceAsync();
        }

        public override Task<long?> GetUnclearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetUnclearedOverallBalanceUntilNowAsync();
        }

        public override Task<IEnumerable<ITransBase>> GetTransPageAsync(int offset, int pageSize)
        {
            return _transRepository.GetPageAsync(offset, pageSize, this);
        }

        public override Task<long> GetTransCountAsync()
        {
            return _transRepository.GetCountAsync(this);
        }
    }
}
