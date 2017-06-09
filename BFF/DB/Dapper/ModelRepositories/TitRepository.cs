using System;
using Persistance = BFF.DB.PersistanceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class TitRepository : ViewRepositoryBase<Domain.Structure.ITitBase, Persistance.TheTit, MVVM.Models.Native.Account>
    {
        public TitRepository(IProvideConnection provideConnection) : base(provideConnection) { }

        protected override Converter<Persistance.TheTit, Domain.Structure.ITitBase> ConvertToDomain => theTit =>
        {
            Enum.TryParse(theTit.Type, true, out Domain.Structure.TitType type);
            Domain.Structure.ITitBase ret;
            switch(type)
            {
                case Domain.Structure.TitType.Transaction:
                    ret = new Domain.Transaction(theTit.Id, theTit.AccountId, theTit.Date, theTit.PayeeId,
                                                 theTit.CategoryId, theTit.Memo, theTit.Sum, theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.Income:
                    ret = new Domain.Income(theTit.Id, theTit.AccountId, theTit.Date, theTit.PayeeId,
                                            theTit.CategoryId, theTit.Memo, theTit.Sum, theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.Transfer:
                    ret = new Domain.Transfer(theTit.Id, theTit.PayeeId, theTit.CategoryId, theTit.Date, theTit.Memo,
                                              theTit.Sum, theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.ParentTransaction:
                    ret = new Domain.ParentTransaction(theTit.Id, theTit.AccountId, theTit.Date, theTit.PayeeId,
                                                       theTit.Memo, theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TitType.ParentIncome:
                    ret = new Domain.ParentIncome(theTit.Id, theTit.AccountId, theTit.Date, theTit.PayeeId, theTit.Memo,
                                                  theTit.Cleared == 1L);
                    break;
                default:
                    ret = new Domain.Transaction(DateTime.Today)
                        {Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR"};
                    break;
            }

            return ret;
        };
        
        protected override string CreateViewStatement => 
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

        protected override string ViewName => "The Tit";

        protected override string GetOrderingPageSuffix(Domain.Account specifyingObject) => $"ORDER BY {nameof(Persistance.TheTit.Date)}";
        
        private string commonSuffix(Domain.Account specifyingObject)
        {
            if(specifyingObject != null && !(specifyingObject is Domain.ISummaryAccount))
                return $"WHERE {nameof(Persistance.TheTit.AccountId)} = {specifyingObject.Id} OR {nameof(Persistance.TheTit.AccountId)} = -69 AND ({nameof(Persistance.TheTit.PayeeId)} = {specifyingObject.Id} OR {nameof(Persistance.TheTit.CategoryId)} = {specifyingObject.Id})";

            return "";
        }

        protected override string GetSpecifyingPageSuffix(Domain.Account specifyingObject)
        {
            return commonSuffix(specifyingObject);
        }

        protected override string GetSpecifyingCountSuffix(Domain.Account specifyingObject)
        {
            return commonSuffix(specifyingObject);
        }
    }
}