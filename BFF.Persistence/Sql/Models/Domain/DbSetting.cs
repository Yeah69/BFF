using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class DbSetting : Model.Models.DbSetting, ISqlModel
    {
        private readonly ICrudOrm<IDbSettingSql> _crudOrm;

        public DbSetting(
            ICrudOrm<IDbSettingSql> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider) : base(rxSchedulerProvider)
        {
            _crudOrm = crudOrm;
        }

        public long Id { get; } = 1;

        public override bool IsInserted => true;
        public override async Task InsertAsync()
        {
            await _crudOrm.CreateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        private IDbSettingSql CreatePersistenceObject()
        {
            return new Persistence.DbSetting
            {
                Id = Id,
                CurrencyCultureName = CurrencyCultureName,
                DateCultureName = DateCultureName
            };
        }
    }
}
