using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class SummaryAccount : Model.Models.SummaryAccount
    {
        private readonly IAccountOrm _accountOrm;
        private readonly ITransRepository _transRepository;

        public SummaryAccount(
            IAccountOrm accountOrm,
            ITransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider) : base(rxSchedulerProvider)
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
