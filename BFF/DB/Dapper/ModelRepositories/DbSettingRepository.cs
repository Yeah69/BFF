using System;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IDbSettingRepository : IRepositoryBase<Domain.IDbSetting>
    {
    }

    public sealed class DbSettingRepository : RepositoryBase<Domain.IDbSetting, DbSetting>, IDbSettingRepository
    {
        public DbSettingRepository(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(provideConnection, crudOrm) { }
        
        protected override Converter<Domain.IDbSetting, DbSetting> ConvertToPersistence => domainDbSetting => 
            new DbSetting
            {
                Id = domainDbSetting.Id,
                CurrencyCultureName = domainDbSetting.CurrencyCultureName,
                DateCultureName = domainDbSetting.DateCultureName
            };

        protected override Converter<DbSetting, Domain.IDbSetting> ConvertToDomain => persistenceDbSetting => 
            new Domain.DbSetting(this, persistenceDbSetting.Id)
            {
                CurrencyCultureName = persistenceDbSetting.CurrencyCultureName,
                DateCultureName = persistenceDbSetting.DateCultureName
            };
    }
}