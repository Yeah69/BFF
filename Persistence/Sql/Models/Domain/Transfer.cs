using System;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Helper;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Transfer : Model.Models.Transfer, ISqlModel
    {
        private readonly ICrudOrm<ITransSql> _crudOrm;

        public Transfer(
            ICrudOrm<ITransSql> crudOrm,
            long id,
            DateTime date,
            IFlag? flag, 
            string checkNumber, 
            IAccount? fromAccount,
            IAccount? toAccount,
            string memo, 
            long sum,
            bool cleared) : base(date, flag, checkNumber, fromAccount, toAccount, memo, sum, cleared)
        {
            Id = id;
            _crudOrm = crudOrm;
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

        private ITransSql CreatePersistenceObject()
        {
            if ((ToAccount is not null || !(ToAccount is Account))
                && (Flag is not null || !(Flag is Flag))
                && (FromAccount is not null || !(FromAccount is Account)))
                throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Trans
            {
                Id = Id,
                Date = Date,
                AccountId = -69,
                CategoryId = (ToAccount as Account)?.Id,
                PayeeId = (FromAccount as Account)?.Id,
                FlagId = (Flag as Flag)?.Id,
                CheckNumber = CheckNumber,
                Memo = Memo,
                Sum = Sum,
                Cleared = Cleared ? 1 : 0,
                Type = nameof(TransType.Transfer)
            };
        }
    }
}
