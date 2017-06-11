using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ICategory : ICommonProperty
    {
        /// <summary>
        /// Id of Parent
        /// </summary>
        long? ParentId { get; set; }
    }

    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public class Category : CommonProperty<Category>, ICategory
    {
        private long? _parentId;

        /// <summary>
        /// Id of Parent
        /// </summary>
        public long? ParentId
        {
            get => _parentId;
            set
            {
                if(_parentId == value) return;
                _parentId = value;
                OnPropertyChanged(); 
            }
        }

        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="name">Name of the Category</param>
        /// <param name="parentId"></param>
        public Category(IRepository<Category> repository, string name = null, long? parentId = null) : base(repository)
        {
            _parentId = parentId;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="parentId">Id of Parent</param>
        /// <param name="name">Name of the Category</param>
        public Category(IRepository<Category> repository, long id, long parentId, string name) : base(repository)
        {
            Id = id;
            ParentId = parentId;
        }
    }
}
