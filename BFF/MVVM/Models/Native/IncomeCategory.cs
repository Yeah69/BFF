using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IIncomeCategory : ICategoryBase
    {
        int MonthOffset { get; set; }
    }

    public class IncomeCategory : CategoryBase<IIncomeCategory>, IIncomeCategory
    {
        private int _monthOffset;

        public int MonthOffset
        {
            get => _monthOffset;
            set
            {
                if (_monthOffset == value) return;
                _monthOffset = value;
                UpdateAndNotify();
            }
        }
        
        public IncomeCategory(
            IRepository<IIncomeCategory> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            long id = -1L, 
            string name = "", 
            int monthOffset = 0) : base(repository, rxSchedulerProvider, id, name)
        {
            _monthOffset = monthOffset;
        }
    }
}
