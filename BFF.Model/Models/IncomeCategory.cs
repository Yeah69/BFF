using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface IIncomeCategory : ICategoryBase
    {
        int MonthOffset { get; set; }

        Task MergeToAsync(IIncomeCategory incomeCategory);
    }

    public abstract class IncomeCategory : CategoryBase, IIncomeCategory
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

        public abstract Task MergeToAsync(IIncomeCategory incomeCategory);

        public IncomeCategory( 
            IRxSchedulerProvider rxSchedulerProvider, 
            string name, 
            int monthOffset) : base(rxSchedulerProvider, name)
        {
            _monthOffset = monthOffset;
        }
    }
}
