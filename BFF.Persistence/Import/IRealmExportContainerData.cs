using System.Collections.Generic;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Import
{
    internal interface IRealmExportContainerData
    {
        IReadOnlyList<Account> Accounts { get; }
        IReadOnlyList<Payee> Payees { get; }
        IReadOnlyList<Category> Categories { get; }
        IReadOnlyList<Category> IncomeCategories { get; }
        IReadOnlyList<Flag> Flags { get; }
        IReadOnlyList<Trans> Trans { get; }
        IReadOnlyList<SubTransaction> SubTransactions { get; }
        IReadOnlyList<BudgetEntry> BudgetEntries { get; }
    }
}