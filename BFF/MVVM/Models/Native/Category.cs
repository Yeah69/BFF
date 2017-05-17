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
    public class Category : CommonProperty, ICategory
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
        public Category( string name = null, long? parentId = null) : base(name)
        {
            _parentId = parentId;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="parentId">Id of Parent</param>
        /// <param name="name">Name of the Category</param>
        public Category(long id, long parentId, string name) : base(name)
        {
            Id = id;
            ParentId = parentId;
        }

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Insert(this);
        }

        public override void Update(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Update(this);
        }

        public override void Delete(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Delete(this);
        }

        #endregion
    }
}
