using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface ISubTransaction : ITransLike, IHaveCategory
    {
        IParentTransaction Parent { get; set; }
        
        long Sum { get; set; }
    }

    internal class SubTransaction<TPersistence> : TransLike<ISubTransaction, TPersistence>, ISubTransaction
        where TPersistence : class, IPersistenceModel
    {
        private IParentTransaction _parent;
        private ICategoryBase _category;
        private long _sum;
        
        public SubTransaction(
            TPersistence backingPersistenceModel,
            IRepository<ISubTransaction, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            bool isInserted = false, 
            ICategoryBase category = null,
            string memo = null,
            long sum = 0L) 
            : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, memo)
        {
            _category = category;
            _sum = sum;
        }

        public IParentTransaction Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;
                _parent = value;
                OnPropertyChanged();
            }
        }
        
        public ICategoryBase Category
        {
            get => _category;
            set
            {
                if (value is null)
                {
                    OnPropertyChanged();
                    return;
                }
                if (_category == value) return;
                _category = value;
                UpdateAndNotify();
            }
        }
        
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = value;
                UpdateAndNotify();
            }
        }

        public override async Task InsertAsync()
        {
            await base.InsertAsync().ConfigureAwait(false);
            Parent.AddSubElement(this);
        }

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync().ConfigureAwait(false);
            Parent.RemoveSubElement(this);
        }
    }
}
