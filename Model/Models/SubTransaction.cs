using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface ISubTransaction : ITransLike, IHaveCategory
    {
        IParentTransaction? Parent { get; set; }
        
        long Sum { get; set; }
    }

    public abstract class SubTransaction : TransLike, ISubTransaction
    {
        private IParentTransaction? _parent;
        private ICategoryBase? _category;
        private long _sum;
        
        public SubTransaction(
            ICategoryBase? category,
            string memo,
            long sum) 
            : base(memo)
        {
            _category = category;
            _sum = sum;
        }

        public IParentTransaction? Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;
                _parent = value;
                OnPropertyChanged();
            }
        }
        
        public ICategoryBase? Category
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

        public override Task InsertAsync()
        {
            Parent?.AddSubElement(this);
            return Task.CompletedTask;
        }

        public override Task DeleteAsync()
        {
            Parent?.RemoveSubElement(this);
            return Task.CompletedTask;
        }
    }
}
