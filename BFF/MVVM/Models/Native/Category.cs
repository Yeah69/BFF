using System;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface ICategory : ICommonProperty
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ObservableCollection<ICategory> Categories { get; set; }

        /// <summary>
        /// The Parent
        /// </summary>
        ICategory Parent { get; set; }

        /// <summary>
        /// Id of Parent
        /// </summary>
        long? ParentId { get; set; }

        string FullName { get; }
    }

    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public class Category : CommonProperty, ICategory
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        [Write(false)]
        public ObservableCollection<ICategory> Categories { get; set; } = new ObservableCollection<ICategory>();

        private ICategory _parent;
        /// <summary>
        /// The Parent
        /// </summary>
        [Write(false)]
        public ICategory Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged();
            }
        }

        private long? _parentId;

        /// <summary>
        /// Id of Parent
        /// </summary>
        public long? ParentId
        {
            get { return _parentId; }
            set
            {
                if(_parentId == value) return;
                _parentId = value;
                OnPropertyChanged(); 
            }
        }

        [Write(false)]
        public string FullName => $"{(_parent != null ? $"{_parent.FullName}." : "")}{Name}";

        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="parent">The Parent</param>
        /// <param name="name">Name of the Category</param>
        public Category(ICategory parent = null, string name = null) : base(name)
        {
            _parent = parent ?? _parent;
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

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString()
        {
            return $"{((Category)Parent)?.GetIndent()}{Name}"; //todo: When there is a CategoryVM transfer this there
        }

        private string GetIndent()
        {
            return $"{((Category)Parent)?.GetIndent()}. "; //todo: When there is a CategoryVM transfer this there
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
