using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentTransactionTable : CreateTableBase
    {
        public CreateParentTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(ParentTransaction)}s](
            {nameof(ParentTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(ParentTransaction.AccountId)} INTEGER,
            {nameof(ParentTransaction.PayeeId)} INTEGER,
            {nameof(ParentTransaction.Date)} DATE,
            {nameof(ParentTransaction.Memo)} TEXT,
            {nameof(ParentTransaction.Cleared)} INTEGER,
            FOREIGN KEY({nameof(ParentTransaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(ParentTransaction.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL);";

    }

    public interface IParentTransactionRepository : IRepositoryBase<Domain.IParentTransaction>
    {
    }

    public class ParentTransactionRepository : RepositoryBase<Domain.IParentTransaction, ParentTransaction>, IParentTransactionRepository
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> _subTransactionsFetcher;

        public ParentTransactionRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> subTransactionsFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _payeeFetcher = payeeFetcher;
            _subTransactionsFetcher = subTransactionsFetcher;
        }

        public override Domain.IParentTransaction Create() =>
            new Domain.ParentTransaction(this, Enumerable.Empty<Domain.SubTransaction>(), -1L, DateTime.MinValue, null, null, "", false);
        
        protected override Converter<Domain.IParentTransaction, ParentTransaction> ConvertToPersistence => domainParentTransaction => 
            new ParentTransaction
            {
                Id = domainParentTransaction.Id,
                AccountId = domainParentTransaction.Account.Id,
                PayeeId = domainParentTransaction.Payee.Id,
                Date = domainParentTransaction.Date,
                Memo = domainParentTransaction.Memo,
                Cleared = domainParentTransaction.Cleared ? 1L : 0L
            };

        protected override Converter<(ParentTransaction, DbConnection), Domain.IParentTransaction> ConvertToDomain => tuple =>
        {
            (ParentTransaction persistenceParentTransaction, DbConnection connection) = tuple;

            var parentTransaction = new Domain.ParentTransaction(
                this,
                _subTransactionsFetcher(persistenceParentTransaction.Id, connection),
                persistenceParentTransaction.Id,
                persistenceParentTransaction.Date,
                _accountFetcher(persistenceParentTransaction.AccountId, connection),
                _payeeFetcher(persistenceParentTransaction.PayeeId, connection),
                persistenceParentTransaction.Memo,
                persistenceParentTransaction.Cleared == 1L);

            foreach (var subTransaction in parentTransaction.SubTransactions)
            {
                subTransaction.Parent = parentTransaction;
            }

            return parentTransaction;
        };
    }
}