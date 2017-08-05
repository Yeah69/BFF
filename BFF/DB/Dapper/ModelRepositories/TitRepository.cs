using System;
using System.Collections.Generic;
using System.Data.Common;
using Persistance = BFF.DB.PersistanceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTitTable : CreateTableBase
    {
        public CreateTitTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected string ViewName => "The Tit";
        
        protected override string CreateTableStatement =>
            $@"CREATE VIEW IF NOT EXISTS [{ViewName}] AS
            SELECT {nameof(Persistance.Transaction.Id)}, {nameof(Persistance.Transaction.AccountId)}, {nameof(Persistance.Transaction.PayeeId)}, {nameof(Persistance.Transaction.CategoryId)}, {nameof(Persistance.Transaction.Date)}, {nameof(Persistance.Transaction.Memo)}, {nameof(Persistance.Transaction.Sum)}, {nameof(Persistance.Transaction.Cleared)}, '{Domain.Structure.TitType.Transaction}' AS Type FROM [{nameof(Persistance.Transaction)}s]
            UNION ALL
            SELECT {nameof(Persistance.ParentTransaction.Id)}, {nameof(Persistance.ParentTransaction.AccountId)}, {nameof(Persistance.ParentTransaction.PayeeId)}, -69 AS CategoryFiller, {nameof(Persistance.ParentTransaction.Date)}, {nameof(Persistance.ParentTransaction.Memo)}, -69 AS SumFiller, {nameof(Persistance.ParentTransaction.Cleared)}, '{Domain.Structure.TitType.ParentTransaction}' AS Type FROM [{nameof(Persistance.ParentTransaction)}s]
            UNION ALL
            SELECT {nameof(Persistance.Income.Id)}, {nameof(Persistance.Income.AccountId)}, {nameof(Persistance.Income.PayeeId)}, {nameof(Persistance.Income.CategoryId)}, {nameof(Persistance.Income.Date)}, {nameof(Persistance.Income.Memo)}, {nameof(Persistance.Income.Sum)}, {nameof(Persistance.Income.Cleared)}, '{Domain.Structure.TitType.Income}' AS Type FROM [{nameof(Persistance.Income)}s]
            UNION ALL
            SELECT {nameof(Persistance.ParentIncome.Id)}, {nameof(Persistance.ParentIncome.AccountId)}, {nameof(Persistance.ParentIncome.PayeeId)}, -69 AS CategoryFiller, {nameof(Persistance.ParentIncome.Date)}, {nameof(Persistance.ParentIncome.Memo)}, -69 AS SumFiller, {nameof(Persistance.ParentIncome.Cleared)}, '{Domain.Structure.TitType.ParentIncome}' AS Type FROM [{nameof(Persistance.ParentIncome)}s]
            UNION ALL
            SELECT {nameof(Persistance.Transfer.Id)}, -69 AS AccountFiller, {nameof(Persistance.Transfer.FromAccountId)}, {nameof(Persistance.Transfer.ToAccountId)}, {nameof(Persistance.Transfer.Date)}, {nameof(Persistance.Transfer.Memo)}, {nameof(Persistance.Transfer.Sum)}, {nameof(Persistance.Transfer.Cleared)}, '{Domain.Structure.TitType.Transfer}' AS Type FROM [{nameof(Persistance.Transfer)}s];";
        
    }
    
    public class TitRepository : ViewRepositoryBase<Domain.Structure.ITitBase, Persistance.TheTit, MVVM.Models.Native.Account>
    {
        private readonly IRepository<Domain.Transaction> _transactionRepository;
        private readonly IRepository<Domain.Income> _incomeRepository;
        private readonly IRepository<Domain.Transfer> _transferRepository;
        private readonly IRepository<Domain.ParentTransaction> _parentTransactionRepository;
        private readonly IRepository<Domain.ParentIncome> _parentIncomeRepository;
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> _subTransactionsFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubIncome>> _subIncomesFetcher;

        public TitRepository(
            IProvideConnection provideConnection, 
            IRepository<Domain.Transaction> transactionRepository, 
            IRepository<Domain.Income> incomeRepository, 
            IRepository<Domain.Transfer> transferRepository, 
            IRepository<Domain.ParentTransaction> parentTransactionRepository,
            IRepository<Domain.ParentIncome> parentIncomeRepository,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> subTransactionsFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubIncome>> subIncomesFetcher)
            : base(provideConnection)
        {
            _transactionRepository = transactionRepository;
            _incomeRepository = incomeRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _parentIncomeRepository = parentIncomeRepository;
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
            _subIncomesFetcher = subIncomesFetcher;
            _subTransactionsFetcher = subTransactionsFetcher;
        }

        protected override Converter<(Persistance.TheTit, DbConnection), Domain.Structure.ITitBase> ConvertToDomain => tuple =>
        {
            (Persistance.TheTit theTit, DbConnection connection) = tuple;
            Enum.TryParse(theTit.Type, true, out Domain.Structure.TitType type);
            Domain.Structure.ITitBase ret;
            switch(type)
            {
                case Domain.Structure.TitType.Transaction:
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
                case Domain.Structure.TitType.Income:
                    ret = new Domain.Income(
                        _incomeRepository, 
                        theTit.Id, 
                        theTit.Date,
                        _accountFetcher(theTit.AccountId, connection),
                        _payeeFetcher(theTit.PayeeId, connection),
                        _categoryFetcher(theTit.CategoryId, connection),
                        theTit.Memo,
                        theTit.Sum, 
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        theTit.Id,
                        theTit.Date,
                        _accountFetcher(theTit.PayeeId, connection),
                        _accountFetcher(theTit.CategoryId, connection), 
                        theTit.Memo,
                        theTit.Sum, 
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.ParentTransaction:
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
                case Domain.Structure.TitType.ParentIncome:
                    ret = new Domain.ParentIncome(
                        _parentIncomeRepository,
                        _subIncomesFetcher(theTit.Id, connection),
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

        protected override string GetOrderingPageSuffix(Domain.Account specifyingObject) => $"ORDER BY {nameof(Persistance.TheTit.Date)}";
        
        protected override string ViewName => "The Tit";
        
        private string CommonSuffix(Domain.Account specifyingObject)
        {
            if(specifyingObject != null && !(specifyingObject is Domain.ISummaryAccount))
                return $"WHERE {nameof(Persistance.TheTit.AccountId)} = {specifyingObject.Id} OR {nameof(Persistance.TheTit.AccountId)} = -69 AND ({nameof(Persistance.TheTit.PayeeId)} = {specifyingObject.Id} OR {nameof(Persistance.TheTit.CategoryId)} = {specifyingObject.Id})";

            return "";
        }

        protected override string GetSpecifyingPageSuffix(Domain.Account specifyingObject)
        {
            return CommonSuffix(specifyingObject);
        }

        protected override string GetSpecifyingCountSuffix(Domain.Account specifyingObject)
        {
            return CommonSuffix(specifyingObject);
        }
    }
}