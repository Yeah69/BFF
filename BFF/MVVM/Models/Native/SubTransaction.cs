using System.Threading.Tasks;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransaction : ITransLike, IHaveCategory
    {
        IParentTransaction Parent { get; set; }
        
        long Sum { get; set; }
    }
    
    public class SubTransaction : TransLike<ISubTransaction>, ISubTransaction
    {
        private IParentTransaction _parent;
        private ICategoryBase _category;
        private long _sum;
        
        public SubTransaction(
            IRepository<ISubTransaction> repository,
            long id = -1L, 
            ICategoryBase category = null,
            string memo = null,
            long sum = 0L) 
            : base(repository, id, memo)
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
