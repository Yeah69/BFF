using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;

namespace BFF.MVVM.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        IDataVirtualizingCollection<IBudgetMonthViewModel> BudgetMonths { get; }
    }

    public class BudgetOverviewViewModel : IBudgetOverviewViewModel
    {
        public IDataVirtualizingCollection<IBudgetMonthViewModel> BudgetMonths { get; }

        public BudgetOverviewViewModel()
        {
            //BudgetMonths = CreateBudgetMonths();
        }

        //private IDataVirtualizingCollection<IBudgetMonthViewModel> CreateBudgetMonths()
        //{
        //    return CollectionBuilder<IBudgetMonthViewModel>
        //        .CreateBuilder()
        //        .BuildAHoardingPreloadingSyncCollection(
        //            new RelayBasicSyncDataAccess<IBudgetMonthViewModel>(
        //                (offset, pageSize) =>
        //                {
        //                    int yearOfFirstMonth = (offset + 1) / 12 + 1;
        //                    int firstMonthNumber = (offset + 1) % 12;
        //                },
        //                () => 119988), 
        //            8);
        //}
    }
}
