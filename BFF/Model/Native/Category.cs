using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
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
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Parent
        /// </summary>
        public long? ParentId
        {
            get { return Parent?.Id; }
            set { _parent = Database?.GetCategory(value ?? -1L); 
            }
        }

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
            return $"{Parent?.getIndent()}{Name}";
        }

        private string getIndent()
        {
            return $"{Parent?.getIndent()}. ";
        }

        private static readonly Dictionary<string, Category> Cache = new Dictionary<string, Category>();
        private Category _parent;

        // todo: Refactor the GetOrCreate and GetAllCache into the Conversion/Import class

        public static Category GetOrCreate(string namePath)
        {
            if (namePath == "")
                return null;
            if (Cache.ContainsKey(namePath))
                return Cache[namePath];
            Stack<string> nameStack = new Stack<string>(namePath.Split(':'));
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category {Name = name, Parent = parentCategory, Categories = new ObservableCollection<Category>() };
            parentCategory?.Categories.Add(category);
            Cache.Add(namePath, category);
            return category;
        }

        public static Category GetOrCreate(Stack<string> nameStack)
        {
            if (nameStack.Count < 1)
                return null;
            string namePath = string.Join(";", nameStack);
            if (Cache.ContainsKey(namePath))
                return Cache[namePath];
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category { Name = name, Parent = parentCategory, Categories = new ObservableCollection<Category>() };
            parentCategory?.Categories.Add(category);
            Cache.Add(namePath, category);
            return category;
        }

        public static List<Category> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }

        protected override void DbUpdate()
        {
            Database?.Update(this);
        }
    }
}
