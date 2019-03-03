using System;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Flag : Model.Models.Flag, ISqlModel
    {
        private readonly ICrudOrm<IFlagSql> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IFlagRepositoryInternal _repository;

        public Flag(
            ICrudOrm<IFlagSql> crudOrm,
            IMergeOrm mergeOrm,
            IFlagRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            Color color, 
            string name) : base(rxSchedulerProvider, color, name)
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

        public override async Task MergeToAsync(IFlag flag)
        {
            if (!(flag is Flag)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(flag));

            await _mergeOrm.MergeFlagAsync(
                    CreatePersistenceObject(),
                    ((Flag)flag).CreatePersistenceObject())
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }

        private IFlagSql CreatePersistenceObject()
        {
            long color = Color.A;
            color = color << 8;
            color = color + Color.R;
            color = color << 8;
            color = color + Color.G;
            color = color << 8;
            color = color + Color.B;

            return new Persistence.Flag
            {
                Id = Id,
                Name = Name,
                Color = color
            };
        }
    }
}
