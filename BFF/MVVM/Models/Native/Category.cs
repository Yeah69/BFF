using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{

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

        public Category(IRepository<ICategory> repository, long id = -1L, string name = "", ICategory parent = null) : base(repository, id, name)
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
