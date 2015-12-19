using System.Collections.ObjectModel;
using BFF.DB;
using BFF.Model.Native;

namespace BFF.Helper.DataProviders
{
    class BudgetElementsProvider : IProvideBudgetElements
    {
        private IDb _database;

        public BudgetElementsProvider(IDb database)
        {
            _database = database;
            if (_database != null)
            {
                AllAccounts = new ObservableCollection<Account>(_database.GetAll<Account>());
                AllPayees = new ObservableCollection<Payee>(_database.GetAll<Payee>());
            }
        }

        public ObservableCollection<Account> AllAccounts { get; }
        public ObservableCollection<Payee> AllPayees { get; }
        public ObservableCollection<Category> AllCategories { get; }
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}
