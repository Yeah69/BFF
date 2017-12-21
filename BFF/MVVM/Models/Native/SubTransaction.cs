using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransaction : ISubTransInc
    {
        /// <summary>
        /// The parent transaction.
        /// </summary>
        IParentTransaction Parent { get; set; }
    }

    /// <summary>
    /// A SubElement of a Transaction
    /// </summary>
    public class SubTransaction : SubTransInc<ISubTransaction>, ISubTransaction
    {
        private IParentTransaction _parent;

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubTransaction(
            IRepository<ISubTransaction> repository,
            long id, 
            ICategoryBase category = null,
            string memo = null,
            long sum = 0L) 
            : base(repository, id, category, memo, sum) {}

        public IParentTransaction Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;
                _parent = value;
                Update();
                OnPropertyChanged();
            }
        }

        public override void Insert()
        {
            base.Insert();
            Parent.AddSubElement(this);
        }

        public override void Delete()
        {
            base.Delete();
            Parent.RemoveSubElement(this);
        }
    }
}
