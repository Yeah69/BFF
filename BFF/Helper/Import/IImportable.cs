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

        public IList<Transaction> Transactions;
        public IList<Income> Incomes;
        public IList<Transfer> Transfers;

        public IList<ParentTransaction> ParentTransactions;
        public IList<ParentIncome> ParentIncomes;

        public IList<SubTransaction> SubTransactions;
        public IList<SubIncome> SubIncomes;

        public IList<BudgetEntry> BudgetEntries;
    }

    public struct ImportAssignments
    {
        public IDictionary<Account, IList<IHaveAccount>> AccountToTransIncBase;
        public IDictionary<Account, IList<Transfer>> FromAccountToTransfer;
        public IDictionary<Account, IList<Transfer>> ToAccountToTransfer;

        public IDictionary<Payee, IList<IHavePayee>> PayeeToTransIncBase;

        public IDictionary<ParentTransaction, IList<SubTransaction>> ParentTransactionToSubTransaction;
        public IDictionary<ParentIncome, IList<SubIncome>> ParentIncomeToSubIncome;
    }
}
