using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportLists
    {
        public IList<Account> Accounts;
        public IList<Payee> Payees;
        public IList<CategoryImportWrapper> Categories;
        public IList<Flag> Flags;

        public IList<Trans> Transactions;
        public IList<Trans> Transfers;

        public IList<Trans> ParentTransactions;

        public IList<SubTransaction> SubTransactions;

        public IList<BudgetEntry> BudgetEntries;
    }
}