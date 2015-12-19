using System.Collections.ObjectModel;
using BFF.Model.Native;

namespace BFF.Helper.DataProviders
{
    interface IProvideBudgetElements
    {
        ObservableCollection<Account> AllAccounts { get; }
        ObservableCollection<Payee> AllPayees { get; }
        ObservableCollection<Category> AllCategories { get; }
        void Reset();
    }
}
