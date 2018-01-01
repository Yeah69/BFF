using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateDbSettingTable : CreateTableBase
    {
        public CreateDbSettingTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(DbSetting)}s](
            {nameof(DbSetting.Id)} INTEGER PRIMARY KEY, 
            {nameof(DbSetting.CurrencyCultureName)} VARCHAR(10),
            {nameof(DbSetting.DateCultureName)} VARCHAR(10));";
    }

    public interface IDbSettingRepository : IRepositoryBase<Domain.IDbSetting>
    {
    }

    public sealed class DbSettingRepository : RepositoryBase<Domain.IDbSetting, DbSetting>, IDbSettingRepository
    {
        public DbSettingRepository(IProvideConnection provideConnection) : base(provideConnection) { }

        public override Domain.IDbSetting Create() =>
            new Domain.DbSetting(this, -1L);
        
        protected override Converter<Domain.IDbSetting, DbSetting> ConvertToPersistence => domainDbSetting => 
            new DbSetting
            {
                Id = domainDbSetting.Id,
                CurrencyCultureName = domainDbSetting.CurrencyCultureName,
                DateCultureName = domainDbSetting.DateCultureName
            };

        protected override Converter<(DbSetting, DbConnection), Domain.IDbSetting> ConvertToDomain => tuple =>
        {
            (DbSetting persistenceDbSetting, _) = tuple;
            return new Domain.DbSetting(this, persistenceDbSetting.Id)
            {
                CurrencyCultureName = persistenceDbSetting.CurrencyCultureName,
                DateCultureName = persistenceDbSetting.DateCultureName
            };
        };
    }
}