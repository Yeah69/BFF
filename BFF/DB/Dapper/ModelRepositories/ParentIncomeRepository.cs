using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentIncomeTable : CreateTableBase
    {
        public CreateParentIncomeTable(IProvideConnection provideConnection) : base(provideConnection)
        {
        }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.ParentIncome)}s](
            {nameof(Persistance.ParentIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.ParentIncome.AccountId)} INTEGER,
            {nameof(Persistance.ParentIncome.PayeeId)} INTEGER,
            {nameof(Persistance.ParentIncome.Date)} DATE,
            {nameof(Persistance.ParentIncome.Memo)} TEXT,
            {nameof(Persistance.ParentIncome.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.ParentIncome.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.ParentIncome.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class ParentIncomeRepository : CachingRepositoryBase<Domain.ParentIncome, Persistance.ParentIncome>
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubIncome>> _subIncomesFetcher;

        public ParentIncomeRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubIncome>> subIncomesFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _payeeFetcher = payeeFetcher;
            _subIncomesFetcher = subIncomesFetcher;
        }

        public override Domain.ParentIncome Create() =>
            new Domain.ParentIncome(this, Enumerable.Empty<Domain.SubIncome>(), -1L, DateTime.MinValue, null, null, "", false);
        
        protected override Converter<Domain.ParentIncome, Persistance.ParentIncome> ConvertToPersistance => domainParentIncome => 
            new Persistance.ParentIncome
            {
                Id = domainParentIncome.Id,
                AccountId = domainParentIncome.Account.Id,
                PayeeId = domainParentIncome.Payee.Id,
                Date = domainParentIncome.Date,
                Memo = domainParentIncome.Memo,
                Cleared = domainParentIncome.Cleared ? 1L : 0L
            };

        protected override Converter<(Persistance.ParentIncome, DbConnection), Domain.ParentIncome> ConvertToDomain => tuple =>
        {
            (Persistance.ParentIncome persistenceParentIncome, DbConnection connection) = tuple;
            return new Domain.ParentIncome(
                this,
                _subIncomesFetcher(persistenceParentIncome.Id, connection),
                persistenceParentIncome.Id,
                persistenceParentIncome.Date,
                _accountFetcher(persistenceParentIncome.AccountId, connection),
                _payeeFetcher(persistenceParentIncome.PayeeId, connection),
                persistenceParentIncome.Memo,
                persistenceParentIncome.Cleared == 1L);
        };
    }
}