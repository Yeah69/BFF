using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportLists
    {
        public IList<IAccountDto> Accounts;
        public IList<IPayeeDto> Payees;
        public IList<CategoryImportWrapper> Categories;
        public IList<IFlagDto> Flags;

        public IList<ITransDto> Transactions;
        public IList<ITransDto> Transfers;

        public IList<ITransDto> ParentTransactions;

        public IList<ISubTransactionDto> SubTransactions;

        public IList<IBudgetEntryDto> BudgetEntries;
    }
}