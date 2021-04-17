using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal sealed class SqliteDbSettingRepository : SqliteRepositoryBase<IDbSetting, IDbSettingSql>, IDbSettingRepository
    {
        private readonly ICrudOrm<IDbSettingSql> _crudOrm;

        public SqliteDbSettingRepository(
            ICrudOrm<IDbSettingSql> crudOrm) : base(crudOrm)
        {
            _crudOrm = crudOrm;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingSql persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new Models.Domain.DbSetting(
                    _crudOrm)
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