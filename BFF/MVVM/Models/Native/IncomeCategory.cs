using BFF.DB;
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
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="name">Name of the Category</param>
        public IncomeCategory(IRepository<IIncomeCategory> repository, long id = -1L, string name = "", int monthOffset = 0) : base(repository, id, name)
        {
            _monthOffset = monthOffset;
        }
    }
}
