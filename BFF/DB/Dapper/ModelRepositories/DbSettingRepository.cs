using System;
using System.Threading.Tasks;
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

        protected override Task<Domain.IDbSetting> ConvertToDomainAsync(DbSetting persistenceModel)
        {
            return Task.FromResult<Domain.IDbSetting>(
                new Domain.DbSetting(this, persistenceModel.Id)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                });
        }
    }
}