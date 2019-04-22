using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal sealed class SqliteDbSettingRepository : SqliteRepositoryBase<IDbSetting, IDbSettingSql>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IDbSettingSql> _crudOrm;

        public SqliteDbSettingRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IDbSettingSql> crudOrm) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingSql persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new Models.Domain.DbSetting(
                    _crudOrm,
                    _rxSchedulerProvider)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                });
        }

        public Task<IDbSetting> GetSetting()
        {
            return FindAsync(1);
        }
    }
}