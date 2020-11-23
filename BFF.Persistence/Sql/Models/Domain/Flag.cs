using System;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Flag : Model.Models.Flag, ISqlModel
    {
        private readonly ICrudOrm<IFlagSql> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly ISqliteFlagRepositoryInternal _repository;

        public Flag(
            ICrudOrm<IFlagSql> crudOrm,
            IMergeOrm mergeOrm,
            ISqliteFlagRepositoryInternal repository,
            long id,
            Color color, 
            string name) : base(color, name)
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

        private IFlagSql CreatePersistenceObject() =>
            new Persistence.Flag
            {
                Id = Id,
                Name = Name,
                Color = Color.ToLong()
            };
    }
}
