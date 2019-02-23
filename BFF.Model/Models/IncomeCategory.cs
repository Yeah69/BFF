using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface IIncomeCategory : ICategoryBase
    {
        int MonthOffset { get; set; }

        Task MergeToAsync(IIncomeCategory incomeCategory);
    }

    internal class IncomeCategory<TPersistence> : CategoryBase<IIncomeCategory, TPersistence>, IIncomeCategory
        where TPersistence : class, IPersistenceModel
    {
        private readonly IMergingRepository<IIncomeCategory> _mergingRepository;
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
            return _mergingRepository.MergeAsync(@from: this, to: incomeCategory);
        }

        public IncomeCategory(
            TPersistence backingPersistenceModel,
            IMergingRepository<IIncomeCategory> mergingRepository,
            IRepository<IIncomeCategory, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            bool isInserted, 
            string name = "", 
            int monthOffset = 0) : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, name)
        {
            _mergingRepository = mergingRepository;
            _monthOffset = monthOffset;
        }
    }
}
