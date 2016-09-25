using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.Helper.Import
{
    interface IImportable
    {
        string SavePath { get; set; }

        string Import();
    }

    public struct ImportLists
    {
        public IList<IAccount> Accounts;
        public IList<IPayee> Payees;
        public IList<CategoryImportWrapper> Categories;

        public IList<ITransaction> Transactions;
        public IList<IIncome> Incomes;
        public IList<ITransfer> Transfers;

        public IList<IParentTransaction> ParentTransactions;
        public IList<IParentIncome> ParentIncomes;

        public IList<ISubTransaction> SubTransactions;
        public IList<ISubIncome> SubIncomes;
    }

    public struct ImportAssignments
    {
        public IDictionary<IAccount, IList<ITransIncBase>> AccountToTransIncBase;
        public IDictionary<IAccount, IList<ITransfer>> FromAccountToTransfer;
        public IDictionary<IAccount, IList<ITransfer>> ToAccountToTransfer;

        public IDictionary<IPayee, IList<ITransIncBase>> PayeeToTransIncBase;

        public IDictionary<IParentTransaction, IList<ISubTransaction>> ParentTransactionToSubTransaction;
        public IDictionary<IParentIncome, IList<ISubIncome>> ParentIncomeToSubIncome;
    }
}
