using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{

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

    internal class Category<TPersistence> : CategoryBase<ICategory, TPersistence>, ICategory
        where TPersistence : class, IPersistenceModel
    {
        private readonly IMergingRepository<ICategory> _mergingRepository;
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
            TPersistence backingPersistenceModel,
            IMergingRepository<ICategory> mergingRepository,
            IRepository<ICategory, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider, 
            bool isInserted,
            string name = "",
            ICategory parent = null) : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, name)
        {
            _mergingRepository = mergingRepository;
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

        public Task MergeToAsync(ICategory category)
        {
            var current = category;
            while (current != null)
            {
                if (current == this) return Task.CompletedTask;
                current = current.Parent;
            }

            return _mergingRepository.MergeAsync(@from: this, to: category);
        }
    }
}
