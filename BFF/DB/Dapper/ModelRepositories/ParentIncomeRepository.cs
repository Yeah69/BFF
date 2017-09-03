using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentIncomeTable : CreateTableBase
    {
        public CreateParentIncomeTable(IProvideConnection provideConnection) : base(provideConnection)
        {
        }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(ParentIncome)}s](
            {nameof(ParentIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(ParentIncome.AccountId)} INTEGER,
            {nameof(ParentIncome.PayeeId)} INTEGER,
            {nameof(ParentIncome.Date)} DATE,
            {nameof(ParentIncome.Memo)} TEXT,
            {nameof(ParentIncome.Cleared)} INTEGER,
            FOREIGN KEY({nameof(ParentIncome.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(ParentIncome.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL);";

    }

    public interface IParentIncomeRepository : IRepositoryBase<Domain.IParentIncome>
    {
    }

    public class ParentIncomeRepository : RepositoryBase<Domain.IParentIncome, ParentIncome>, IParentIncomeRepository
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

        public override Domain.IParentIncome Create() =>
            new Domain.ParentIncome(this, Enumerable.Empty<Domain.SubIncome>(), -1L, DateTime.MinValue, null, null, "", false);
        
        protected override Converter<Domain.IParentIncome, ParentIncome> ConvertToPersistence => domainParentIncome => 
            new ParentIncome
            {
                Id = domainParentIncome.Id,
                AccountId = domainParentIncome.Account.Id,
                PayeeId = domainParentIncome.Payee.Id,
                Date = domainParentIncome.Date,
                Memo = domainParentIncome.Memo,
                Cleared = domainParentIncome.Cleared ? 1L : 0L
            };

        protected override Converter<(ParentIncome, DbConnection), Domain.IParentIncome> ConvertToDomain => tuple =>
        {
            (ParentIncome persistenceParentIncome, DbConnection connection) = tuple;
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