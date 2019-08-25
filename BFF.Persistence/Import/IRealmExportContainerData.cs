using System.Collections.Generic;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Import
{
    public interface IRealmExportContainerData
    {
        IReadOnlyList<IAccountRealm> Accounts { get; }
        IReadOnlyList<IPayeeRealm> Payees { get; }
        IReadOnlyList<ICategoryRealm> RootCategories { get; }
        IReadOnlyList<ICategoryRealm> IncomeCategories { get; }
        IReadOnlyList<IFlagRealm> Flags { get; }
        IReadOnlyList<ITransRealm> Trans { get; }
        IReadOnlyList<ISubTransactionRealm> SubTransactions { get; }
        IReadOnlyList<IBudgetEntryRealm> BudgetEntries { get; }
    }
}