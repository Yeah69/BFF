using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ICategoryBase : ICommonProperty
    {
    }

    public interface ICategory : ICategoryBase
    {
        /// <summary>
        /// Id of Parent
        /// </summary>
        ICategory Parent { get; set; }

        ReadOnlyObservableCollection<ICategory> Categories { get; }

        void AddCategory(ICategory category);

        void RemoveCategory(ICategory category);
    }

    public interface IIncomeCategory: ICategoryBase
    {
        int MonthOffset { get; set; }
    }

    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public class CategoryBase<T> : CommonProperty<T>, ICategoryBase where T : class, ICategoryBase
    {
        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="name">Name of the Category</param>
        public CategoryBase(IRepository<T> repository, long id, string name) : base(repository, id, name)
        {
        }
    }

    public class Category : CategoryBase<ICategory>, ICategory
    {
        private ICategory _parent;
        private readonly ObservableCollection<ICategory> _categories = new ObservableCollection<ICategory>();

        /// <summary>
        /// Id of Parent
        /// </summary>
        public ICategory Parent
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

        public ReadOnlyObservableCollection<ICategory> Categories { get; }

        public Category(IRepository<ICategory> repository, long id, string name, ICategory parent) : base(repository, id, name)
        {
            _parent = parent;
            Categories = new ReadOnlyObservableCollection<ICategory>(_categories);
        }

        public void AddCategory(ICategory category)
        {
            _categories.Add(category);
        }

        public void RemoveCategory(ICategory category)
        {
            _categories.Remove(category);
        }
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
        public IncomeCategory(IRepository<IIncomeCategory> repository, long id, string name, int monthOffset) : base(repository, id, name)
        {
            _monthOffset = monthOffset;
        }
    }
}
