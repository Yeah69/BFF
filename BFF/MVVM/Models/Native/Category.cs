﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
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

        Task MergeTo(ICategory category);
    }

    public class Category : CategoryBase<ICategory>, ICategory
    {
        private readonly ICategoryRepository _repository;
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

        public ReadOnlyObservableCollection<ICategory> Categories { get; }

        public Category(
            ICategoryRepository repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            long id = -1L,
            string name = "",
            ICategory parent = null) : base(repository, rxSchedulerProvider, id, name)
        {
            _repository = repository;
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

        public Task MergeTo(ICategory category)
        {
            var current = category;
            while (current != null)
            {
                if (current == this) return Task.CompletedTask;
                current = current.Parent;
            }

            return _repository.MergeAsync(from: this, to: category);
        }
    }
}
