using System;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IDbSettingRepository : IRepositoryBase<IDbSetting>
    {
    }

    public sealed class DbSettingRepository : RepositoryBase<IDbSetting, DbSettingDto>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public DbSettingRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
        }
        
        protected override Converter<IDbSetting, DbSettingDto> ConvertToPersistence => domainDbSetting => 
            new DbSettingDto
            {
                Id = domainDbSetting.Id,
                CurrencyCultureName = domainDbSetting.CurrencyCultureName,
                DateCultureName = domainDbSetting.DateCultureName
            };

        protected override Task<IDbSetting> ConvertToDomainAsync(DbSettingDto persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new DbSetting(this, _rxSchedulerProvider, persistenceModel.Id)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                });
        }
    }
}