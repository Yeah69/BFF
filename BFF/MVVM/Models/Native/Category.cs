using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ICategory : ICommonProperty
    {
        /// <summary>
        /// Id of Parent
        /// </summary>
        ICategory Parent { get; set; }

        ReadOnlyObservableCollection<ICategory> Categories { get; }

        void AddCategory(ICategory category);

        void RemoveCategory(ICategory category);
    }

    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public class Category : CommonProperty<ICategory>, ICategory
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
                if(_parent == value) return;
                _parent = value;
                Update();
                OnPropertyChanged(); 
            }
        }

        public ReadOnlyObservableCollection<ICategory> Categories { get; }

        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="name">Name of the Category</param>
        /// <param name="parentId"></param>
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
}
