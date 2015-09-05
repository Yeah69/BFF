using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Category : DataModelBase
    {
        #region Non-Static

        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public string Name { get; set; }

        //todo:[DataField]
        public List<Category> Categories { get; set; }

        //todo:[DataField]
        public Category ParentCategory { get; set; }

        #endregion

        #region Methods

        protected override string GetDelimitedCreateTableList(string delimiter)
        {
            List<string> list = new List<string>{"ID INTEGER PRIMARY KEY AUTOINCREMENT",
                "Name VARCHAR(100)"
            };
            return string.Join(delimiter, list);
        }

        #endregion

        #endregion

        #region Static

        #region Static Variables

        private static readonly Dictionary<string, Category> Cache = new Dictionary<string, Category>();

        #endregion

        #region Static Methods

        public static Category GetOrCreate(string namePath)
        {
            if (Cache.ContainsKey(namePath))
                return Cache[namePath];
            //todo: Find out about converting the ID
            Stack<string> nameStack = new Stack<string>(namePath.Split(';'));
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category { ID = 69, Name = name, ParentCategory = parentCategory, Categories = new List<Category>() };
            if(parentCategory != null)
                parentCategory.Categories.Add(category);
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
            //todo: Find out about converting the ID
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category { ID = 69, Name = name, ParentCategory = parentCategory, Categories = new List<Category>() };
            if (parentCategory != null)
                parentCategory.Categories.Add(category);
            Cache.Add(namePath, category);
            return category;
        }

        public static List<Category> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        #endregion

        #endregion
    }
}
