using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public class CategoryComparer : Comparer<ICategory>
    {
        public override int Compare(ICategory x, ICategory y)
        {
            IList<ICategory> GetParentalPathList(ICategory category)
            {
                IList<ICategory> list = new List<ICategory> { category };
                ICategory current = category;
                while (current.Parent != null)
                {
                    current = current.Parent;
                    list.Add(current);
                }

                return list.Reverse().ToList();
            }

            IList<ICategory> xList = GetParentalPathList(x);
            IList<ICategory> yList = GetParentalPathList(y);

            int i = 0;
            int value = 0;
            while (value == 0)
            {
                if (i >= xList.Count && i >= yList.Count) return 0;
                if (i >= xList.Count) return -1;
                if (i >= yList.Count) return 1;

                value = Comparer<string>.Default.Compare(xList[i].Name, yList[i].Name);
                i++;
            }

            return value;
        }
    }

    public interface ICategory : ICategoryBase
    {
        /// <summary>
        /// Id of Parent
        /// </summary>
        ICategory Parent { get; set; }

        void SetParentWithoutUpdateToBackend(ICategory newParent);

        ReadOnlyObservableCollection<ICategory> Categories { get; }

        bool IsMyAncestor(ICategory other);

        void AddCategory(ICategory category);

        void RemoveCategory(ICategory category);

        Task MergeToAsync(ICategory category);
    }

    public abstract class Category : CategoryBase, ICategory
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
                UpdateAndNotify();
            }
        }

        public void SetParentWithoutUpdateToBackend(ICategory newParent)
        {
            _parent = newParent;
            OnPropertyChanged(nameof(Parent));
        }

        public ReadOnlyObservableCollection<ICategory> Categories { get; }

        public Category(
            string name,
            ICategory parent) : base(name)
        {
            _parent = parent;
            Categories = new ReadOnlyObservableCollection<ICategory>(_categories);
        }

        public bool IsMyAncestor(ICategory other)
        {
            var current = Parent;
            while (current != null)
            {
                if (current == other) return true;
                current = current.Parent;
            }

            return false;
        }

        public void AddCategory(ICategory category)
        {
            _categories.Add(category);
        }

        public void RemoveCategory(ICategory category)
        {
            _categories.Remove(category);
        }

        public abstract Task MergeToAsync(ICategory category);

        public override Task DeleteAsync()
        {
            Parent.RemoveCategory(this);
            return Task.CompletedTask;
        }
    }
}
