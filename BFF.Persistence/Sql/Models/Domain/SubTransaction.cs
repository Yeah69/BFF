using System;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class SubTransaction : Model.Models.SubTransaction, ISqlModel
    {
        private readonly ICrudOrm<ISubTransactionSql> _crudOrm;

        public SubTransaction(
            ICrudOrm<ISubTransactionSql> crudOrm,
            long id,
            ICategoryBase? category,
            string memo, 
            long sum) : base(category, memo, sum)
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

        private ISubTransactionSql CreatePersistenceObject()
        {
            if (!(Parent is ParentTransaction)
                || Category != null && !(Category is Category))
                throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Persistence.SubTransaction
            {
                Id = Id,
                ParentId = ((ParentTransaction) Parent).Id,
                CategoryId = (Category as Category)?.Id,
                Memo = Memo,
                Sum = Sum
            };
        }
    }
}
