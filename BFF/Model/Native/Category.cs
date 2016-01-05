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
        [Key]
        public long Id { get; set; } = -1;

        public string Name { get; set; }

        [Write(false)]
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        [Write(false)]
        public Category Parent { get; set; }

        public long? ParentId
        {
            get { return Parent?.Id; }
            set { //Parent = Database?.GetCategory(value ?? -1L); 
            } //todo: Maybe set this as Parent's child
        }

        public override string ToString()
        {
            return Name;
        }

        private static readonly Dictionary<string, Category> Cache = new Dictionary<string, Category>();

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
    }
}
