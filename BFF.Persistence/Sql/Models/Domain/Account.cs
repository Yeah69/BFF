using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Account : Model.Models.Account, ISqlModel
    {
        private readonly ICrudOrm<IAccountSql> _crudOrm;
        private readonly IAccountOrm _accountOrm;
        private readonly ITransRepository _transRepository;

        public Account(
            ICrudOrm<IAccountSql> crudOrm,
            IAccountOrm accountOrm,
            ITransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            DateTime startingDate, 
            string name, 
            long startingBalance) : base(rxSchedulerProvider, startingDate, name, startingBalance)
        {
            Id = id;
            _crudOrm = crudOrm;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
        }

        public long Id { get; private set; }

        public override bool IsInserted => Id > 0;
        public override async Task InsertAsync()
        {
            Id = await _crudOrm.CreateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        private IAccountSql CreatePersistenceObject()
        {
            return new Persistence.Account
            {
                Id = Id,
                Name = Name,
                StartingDate = StartingDate,
                StartingBalance = StartingBalance
            };
        }

        public override Task<long?> GetClearedBalanceAsync()
        {
            return _accountOrm.GetClearedBalanceAsync(Id);
        }

        public override Task<long?> GetClearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetClearedBalanceUntilNowAsync(Id);
        }

        public override Task<long?> GetUnclearedBalanceAsync()
        {
            return _accountOrm.GetUnclearedBalanceAsync(Id);
        }

        public override Task<long?> GetUnclearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetUnclearedBalanceUntilNowAsync(Id);
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
