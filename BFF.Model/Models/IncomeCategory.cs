using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories.ModelRepositories;

namespace BFF.Model.Models
{
    public interface IIncomeCategory : ICategoryBase
    {
        int MonthOffset { get; set; }

        Task MergeToAsync(IIncomeCategory incomeCategory);
    }

    internal class IncomeCategory : CategoryBase<IIncomeCategory>, IIncomeCategory
    {
        private readonly IIncomeCategoryRepository _repository;
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

        public Task MergeToAsync(IIncomeCategory incomeCategory)
        {
            return _repository.MergeAsync(@from: this, to: incomeCategory);
        }

        public IncomeCategory(
            IIncomeCategoryRepository repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            long id = -1L, 
            string name = "", 
            int monthOffset = 0) : base(repository, rxSchedulerProvider, id, name)
        {
            _repository = repository;
            _monthOffset = monthOffset;
        }
    }
}
