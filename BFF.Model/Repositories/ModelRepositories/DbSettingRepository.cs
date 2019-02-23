using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IDbSettingRepository
    {
        Task<IDbSetting> GetSetting();
    }

    internal sealed class DbSettingRepository : RepositoryBase<IDbSetting, IDbSettingSql>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public DbSettingRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IDbSettingSql> crudOrm) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingSql persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new DbSetting<IDbSettingSql>(
                    persistenceModel, 
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Id > 0)
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