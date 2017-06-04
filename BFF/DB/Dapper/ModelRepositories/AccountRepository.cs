using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class AccountRepository : RepositoryBase<Domain.Account, Persistance.Account>
    {
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Account)}s](
            {nameof(Persistance.Account.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Account.Name)} VARCHAR(100),
            {nameof(Persistance.Account.StartingBalance)} INTEGER NOT NULL DEFAULT 0);";
        
        public AccountRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        protected override Converter<Domain.Account, Persistance.Account> ConvertToPersistance => domainAccount => 
            new Persistance.Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance
            };
        
        protected override Converter<Persistance.Account, Domain.Account> ConvertToDomain => persistanceAccount =>
            new Domain.Account
            {
                Id = persistanceAccount.Id,
                Name = persistanceAccount.Name,
                StartingBalance = persistanceAccount.StartingBalance
            };
    }
}