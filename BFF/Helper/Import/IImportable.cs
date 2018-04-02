using System.Collections.Generic;
using BFF.DB.PersistenceModels;

namespace BFF.Helper.Import
{
    public interface IImportable
    {
        string SavePath { get; set; }

        string Import();
    }

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

    public struct ImportAssignments
    {
        public IDictionary<Account, IList<IHaveAccount>> AccountToTransactionBase;
        public IDictionary<Account, IList<Trans>> FromAccountToTransfer;
        public IDictionary<Account, IList<Trans>> ToAccountToTransfer;

        public IDictionary<Payee, IList<IHavePayee>> PayeeToTransactionBase;

        public IDictionary<Trans, IList<SubTransaction>> ParentTransactionToSubTransaction;
        
        public IDictionary<Flag, IList<IHaveFlag>> FlagToTransBase;
    }
}
