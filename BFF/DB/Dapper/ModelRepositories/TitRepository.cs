using System;
using System.Collections.Generic;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTitTable : CreateTableBase
    {
        public CreateTitTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected string ViewName => "The Tit";
        
        protected override string CreateTableStatement =>
            $@"CREATE VIEW IF NOT EXISTS [{ViewName}] AS
            SELECT {nameof(Transaction.Id)}, {nameof(Transaction.AccountId)}, {nameof(Transaction.PayeeId)}, {nameof(Transaction.CategoryId)}, {nameof(Transaction.Date)}, {nameof(Transaction.Memo)}, {nameof(Transaction.Sum)}, {nameof(Transaction.Cleared)}, '{Domain.Structure.TransType.Transaction}' AS Type FROM [{nameof(Transaction)}s]
            UNION ALL
            SELECT {nameof(ParentTransaction.Id)}, {nameof(ParentTransaction.AccountId)}, {nameof(ParentTransaction.PayeeId)}, -69 AS CategoryFiller, {nameof(ParentTransaction.Date)}, {nameof(ParentTransaction.Memo)}, -69 AS SumFiller, {nameof(ParentTransaction.Cleared)}, '{Domain.Structure.TransType.ParentTransaction}' AS Type FROM [{nameof(ParentTransaction)}s]
            UNION ALL
            SELECT {nameof(Transfer.Id)}, -69 AS AccountFiller, {nameof(Transfer.FromAccountId)}, {nameof(Transfer.ToAccountId)}, {nameof(Transfer.Date)}, {nameof(Transfer.Memo)}, {nameof(Transfer.Sum)}, {nameof(Transfer.Cleared)}, '{Domain.Structure.TransType.Transfer}' AS Type FROM [{nameof(Transfer)}s];";
        
    }

    public interface ITitRepository : IViewRepositoryBase<Domain.Structure.ITransBase, MVVM.Models.Native.IAccount>
    {
    }


    public sealed class TitRepository : ViewRepositoryBase<Domain.Structure.ITransBase, TheTit, MVVM.Models.Native.IAccount>, ITitRepository
    {

        private readonly IRepository<Domain.ITransaction> _transactionRepository;
        private readonly IRepository<Domain.ITransfer> _transferRepository;
        private readonly IRepository<Domain.IParentTransaction> _parentTransactionRepository;
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long?, DbConnection, Domain.ICategoryBase> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> _subTransactionsFetcher;

        public TitRepository(
            IProvideConnection provideConnection, 
            IRepository<Domain.ITransaction> transactionRepository, 
            IRepository<Domain.ITransfer> transferRepository, 
            IRepository<Domain.IParentTransaction> parentTransactionRepository,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long?, DbConnection, Domain.ICategoryBase> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> subTransactionsFetcher)
            : base(provideConnection)
        {
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
            _subTransactionsFetcher = subTransactionsFetcher;
        }

        protected override Converter<(TheTit, DbConnection), Domain.Structure.ITransBase> ConvertToDomain => tuple =>
        {
            (TheTit theTit, DbConnection connection) = tuple;
            Enum.TryParse(theTit.Type, true, out Domain.Structure.TransType type);
            Domain.Structure.ITransBase ret;
            switch(type)
            {
                case Domain.Structure.TransType.Transaction:
                    ret = new Domain.Transaction(
                        _transactionRepository, 
                        theTit.Id,
                        theTit.Date,
                        _accountFetcher(theTit.AccountId, connection),
                        _payeeFetcher(theTit.PayeeId, connection),
                        _categoryFetcher(theTit.CategoryId, connection), 
                        theTit.Memo, 
                        theTit.Sum,
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        theTit.Id,
                        theTit.Date,
                        _accountFetcher(theTit.PayeeId, connection),
                        _accountFetcher(theTit.CategoryId ?? -1, connection), // This CategoryId should never be a null, because it comes from a transfer
                        theTit.Memo,
                        theTit.Sum, 
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.ParentTransaction:
                    ret = new Domain.ParentTransaction(
                        _parentTransactionRepository,
                        _subTransactionsFetcher(theTit.Id, connection), 
                        theTit.Id,
                        theTit.Date,
                        _accountFetcher(theTit.AccountId, connection),
                        _payeeFetcher(theTit.PayeeId, connection),
                        theTit.Memo, 
                        theTit.Cleared == 1L);
                    break;
                default:
                    ret = new Domain.Transaction(_transactionRepository, -1L, DateTime.Today)
                        {Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR"};
                    break;
            }

            return ret;
        };

        protected override string GetOrderingPageSuffix(Domain.IAccount specifyingObject) => $"ORDER BY {nameof(TheTit.Date)}";
        
        protected override string ViewName => "The Tit";
        
        private string CommonSuffix(Domain.IAccount specifyingObject)
        {
            if(specifyingObject != null && !(specifyingObject is Domain.ISummaryAccount))
                return $"WHERE {nameof(TheTit.AccountId)} = {specifyingObject.Id} OR {nameof(TheTit.AccountId)} = -69 AND ({nameof(TheTit.PayeeId)} = {specifyingObject.Id} OR {nameof(TheTit.CategoryId)} = {specifyingObject.Id})";

            return "";
        }

        protected override string GetSpecifyingPageSuffix(Domain.IAccount specifyingObject)
        {
            return CommonSuffix(specifyingObject);
        }

        protected override string GetSpecifyingCountSuffix(Domain.IAccount specifyingObject)
        {
            return CommonSuffix(specifyingObject);
        }
    }
}