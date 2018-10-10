using System;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IDbSettingRepository : IRepositoryBase<Domain.IDbSetting>
    {
    }

    public sealed class DbSettingRepository : RepositoryBase<Domain.IDbSetting, DbSetting>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public DbSettingRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
        }
        
        protected override Converter<Domain.IDbSetting, DbSetting> ConvertToPersistence => domainDbSetting => 
            new DbSetting
            {
                Id = domainDbSetting.Id,
                CurrencyCultureName = domainDbSetting.CurrencyCultureName,
                DateCultureName = domainDbSetting.DateCultureName
            };

        protected override Task<Domain.IDbSetting> ConvertToDomainAsync(DbSetting persistenceModel)
        {
            return Task.FromResult<Domain.IDbSetting>(
                new Domain.DbSetting(this, _rxSchedulerProvider, persistenceModel.Id)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                });
        }
    }
}