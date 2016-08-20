using System.Collections.ObjectModel;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public class Category : CommonProperty
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        [Write(false)]
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private Category _parent;
        /// <summary>
        /// The Parent
        /// </summary>
        [Write(false)]
        public Category Parent
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
        public Category(Category parent = null, string name = null) : base(name)
        {
            ConstrDbLock = true;

            _parent = parent ?? _parent;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="parentId">Id of Parent</param>
        /// <param name="name">Name of the Category</param>
        public Category(long id, long parentId, string name) : base(name)
        {
            ConstrDbLock = true;

            Id = id;
            ParentId = parentId;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString()
        {
            return $"{Parent?.GetIndent()}{Name}";
        }

        private string GetIndent()
        {
            return $"{Parent?.GetIndent()}. ";
        }

        protected override void InsertToDb()
        {
            Database?.CommonPropertyProvider.Add(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
        }

        protected override void DeleteFromDb()
        {
            Database?.CommonPropertyProvider.Remove(this);
        }
    }
}
