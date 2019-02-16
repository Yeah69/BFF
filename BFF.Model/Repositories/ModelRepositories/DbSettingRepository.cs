using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IDbSettingRepository : IRepositoryBase<IDbSetting>
    {
        Task<IDbSetting> GetSetting();
    }

    internal sealed class DbSettingRepository : RepositoryBase<IDbSetting, IDbSettingSql>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Func<IDbSettingSql> _dbSettingDtoFactory;

        public DbSettingRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            Func<IDbSettingSql> dbSettingDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _dbSettingDtoFactory = dbSettingDtoFactory;
        }
        
        protected override Converter<IDbSetting, IDbSettingSql> ConvertToPersistence => domainDbSetting =>
        {
            var dbSettingDto = _dbSettingDtoFactory();

            dbSettingDto.Id = domainDbSetting.Id;
            dbSettingDto.CurrencyCultureName = domainDbSetting.CurrencyCultureName;
            dbSettingDto.DateCultureName = domainDbSetting.DateCultureName;

            return dbSettingDto;
        };

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingSql persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new DbSetting(this, _rxSchedulerProvider, persistenceModel.Id)
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