using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubIncome : ISubTransInc
    {
        IParentIncome Parent { get; set; }
    }

    /// <summary>
    /// A SubElement of an Income
    /// </summary>
    public class SubIncome : SubTransInc<ISubIncome>, ISubIncome
    {
        private IParentIncome _parent;

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubIncome(
            IRepository<ISubIncome> repository,
            long id,
            ICategoryBase category = null, 
            string memo = null, 
            long sum = 0L) 
            : base(repository, id, category, memo, sum) {}

        public IParentIncome Parent
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
        
        public override void Delete()
        {
            base.Delete();
            if (Parent.SubIncomes.Contains(this))
                Parent.SubIncomes.Remove(this);
        }
    }
}
