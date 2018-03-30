using System.Threading.Tasks;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransaction : ITransLike, IHaveCategory
    {
        /// <summary>
        /// The parent transaction.
        /// </summary>
        IParentTransaction Parent { get; set; }

        /// <summary>
        /// The amount of money, which was payed or received
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// A SubElement of a Transaction
    /// </summary>
    public class SubTransaction : TransLike<ISubTransaction>, ISubTransaction
    {
        private IParentTransaction _parent;
        private ICategoryBase _category;
        private long _sum;

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
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
                //Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
        /// </summary>
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

        /// <summary>
        /// The amount of money, which was payed or received
        /// </summary>
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
            await base.InsertAsync();
            Parent.AddSubElement(this);
        }

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync();
            Parent.RemoveSubElement(this);
        }
    }
}
