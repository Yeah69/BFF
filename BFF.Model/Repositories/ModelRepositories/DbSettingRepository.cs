using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IDbSettingRepository : IRepositoryBase<IDbSetting>
    {
    }

    internal sealed class DbSettingRepository : RepositoryBase<IDbSetting, IDbSettingDto>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Func<IDbSettingDto> _dbSettingDtoFactory;

        public DbSettingRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            Func<IDbSettingDto> dbSettingDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _dbSettingDtoFactory = dbSettingDtoFactory;
        }
        
        protected override Converter<IDbSetting, IDbSettingDto> ConvertToPersistence => domainDbSetting =>
        {
            var dbSettingDto = _dbSettingDtoFactory();

            dbSettingDto.Id = domainDbSetting.Id;
            dbSettingDto.CurrencyCultureName = domainDbSetting.CurrencyCultureName;
            dbSettingDto.DateCultureName = domainDbSetting.DateCultureName;

            return dbSettingDto;
        };

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingDto persistenceModel)
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