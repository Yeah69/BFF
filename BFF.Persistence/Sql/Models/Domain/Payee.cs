using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Payee : Model.Models.Payee, ISqlModel
    {
        private readonly ICrudOrm<IPayeeSql> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IPayeeRepositoryInternal _repository;

        public Payee(
            ICrudOrm<IPayeeSql> crudOrm,
            IMergeOrm mergeOrm,
            IPayeeRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            string name) : base(rxSchedulerProvider, name)
        {
            Id = id;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
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

        public override async Task MergeToAsync(IPayee payee)
        {
            if (!(payee is Payee)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(payee));

            await _mergeOrm.MergePayeeAsync(
                    CreatePersistenceObject(),
                    ((Payee)payee).CreatePersistenceObject())
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }

        private IPayeeSql CreatePersistenceObject()
        {
            return new Persistence.Payee
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
