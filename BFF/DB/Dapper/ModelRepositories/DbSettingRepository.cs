using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateDbSettingTable : CreateTableBase
    {
        public CreateDbSettingTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.DbSetting)}s](
            {nameof(Persistance.DbSetting.Id)} INTEGER PRIMARY KEY, 
            {nameof(Persistance.DbSetting.CurrencyCultureName)} VARCHAR(10),
            {nameof(Persistance.DbSetting.DateCultureName)} VARCHAR(10));";
    }
    
    public class DbSettingRepository : RepositoryBase<Domain.DbSetting, Persistance.DbSetting>
    {
        public DbSettingRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.DbSetting, Persistance.DbSetting> ConvertToPersistance => domainDbSetting => 
            new Persistance.DbSetting
            {
                Id = domainDbSetting.Id,
                CurrencyCultureName = domainDbSetting.CurrencyCultureName,
                DateCultureName = domainDbSetting.DateCultureName
            };
        
        protected override Converter<Persistance.DbSetting, Domain.DbSetting> ConvertToDomain => persistanceDbSetting =>
            new Domain.DbSetting
            {
                Id = persistanceDbSetting.Id,
                CurrencyCultureName = persistanceDbSetting.CurrencyCultureName,
                DateCultureName = persistanceDbSetting.DateCultureName
            };
    }
}