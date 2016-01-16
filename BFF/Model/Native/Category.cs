using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class Category : CommonProperties
    {
        //todo: Db Updates
        public string Name { get; set; }

        [Write(false)]
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        [Write(false)]
        public Category Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                Database?.Update(this);
            }
        }

        public long? ParentId
        {
            get { return Parent?.Id; }
            set { _parent = Database?.GetCategory(value ?? -1L); 
            } //todo: Maybe set this as Parent's child
        }

        public Category(Category parent = null)
        {
            _parent = parent ?? _parent;
        }

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
    }
}
