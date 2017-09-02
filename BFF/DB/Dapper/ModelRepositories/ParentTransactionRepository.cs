using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentTransactionTable : CreateTableBase
    {
        public CreateParentTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.ParentTransaction)}s](
            {nameof(Persistance.ParentTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.ParentTransaction.AccountId)} INTEGER,
            {nameof(Persistance.ParentTransaction.PayeeId)} INTEGER,
            {nameof(Persistance.ParentTransaction.Date)} DATE,
            {nameof(Persistance.ParentTransaction.Memo)} TEXT,
            {nameof(Persistance.ParentTransaction.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.ParentTransaction.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.ParentTransaction.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class ParentTransactionRepository : RepositoryBase<Domain.IParentTransaction, Persistance.ParentTransaction>
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
        
        protected override Converter<Domain.IParentTransaction, Persistance.ParentTransaction> ConvertToPersistance => domainParentTransaction => 
            new Persistance.ParentTransaction
            {
                Id = domainParentTransaction.Id,
                AccountId = domainParentTransaction.Account.Id,
                PayeeId = domainParentTransaction.Payee.Id,
                Date = domainParentTransaction.Date,
                Memo = domainParentTransaction.Memo,
                Cleared = domainParentTransaction.Cleared ? 1L : 0L
            };

        protected override Converter<(Persistance.ParentTransaction, DbConnection), Domain.IParentTransaction> ConvertToDomain => tuple =>
        {
            (Persistance.ParentTransaction persistenceParentTransaction, DbConnection connection) = tuple;

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