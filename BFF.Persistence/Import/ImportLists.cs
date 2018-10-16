using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportLists
    {
        public IList<AccountDto> Accounts;
        public IList<PayeeDto> Payees;
        public IList<CategoryImportWrapper> Categories;
        public IList<FlagDto> Flags;

        public IList<TransDto> Transactions;
        public IList<TransDto> Transfers;

        public IList<TransDto> ParentTransactions;

        public IList<SubTransactionDto> SubTransactions;

        public IList<BudgetEntryDto> BudgetEntries;
    }
}