using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Transaction : Model.Models.Transaction, ISqlModel
    {
        private readonly ICrudOrm<ITransSql> _crudOrm;

        public Transaction(
            ICrudOrm<ITransSql> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            DateTime date,
            IFlag flag,
            string checkNumber,
            IAccount account,
            IPayee payee, 
            ICategoryBase category, 
            string memo, 
            long sum, 
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, account, payee, category, memo, sum, cleared)
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
            if (!(Account is Account)
                && (Category != null || !(Category is Category))
                && (Flag != null || !(Flag is Flag))
                && (Payee != null || !(Payee is Payee)))
                throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Trans
            {
                Id = Id,
                Date = Date,
                AccountId = ((Account)Account).Id,
                CategoryId = (Category as Category)?.Id,
                PayeeId = (Payee as Payee)?.Id,
                FlagId = (Flag as Flag)?.Id,
                CheckNumber = CheckNumber,
                Memo = Memo,
                Sum = Sum,
                Cleared = Cleared ? 1 : 0,
                Type = nameof(TransType.Transaction)
            };
        }
    }
}
