using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using static BFF.DB.SQLite.Helper;

namespace BFF.Model.Native
{
    class Category : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Key]
        public override long Id { get; set; } = -1;

        public string Name { get; set; }

        [Write(false)]
        public List<Category> Categories { get; set; }

        [Write(false)]
        public Category Parent { get; set; }

        public long? ParentId
        {
            get { return Parent?.Id; }
            set { Parent = GetFromDb(value); } //todo: Maybe set this as Parent's child
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #endregion

        #region Static

        #region Static Variables

        private static readonly Dictionary<string, Category> Cache = new Dictionary<string, Category>();
        private static readonly Dictionary<long, Category> DbCache = new Dictionary<long, Category>();

        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Category)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(ParentId)} INTEGER,
                        {nameof(Name)} VARCHAR(100),
                        FOREIGN KEY({nameof(ParentId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        #endregion

        #region Static Methods

        public static Category GetOrCreate(string namePath)
        {
            if (namePath == "")
                return null;
            if (Cache.ContainsKey(namePath))
                return Cache[namePath];
            Stack<string> nameStack = new Stack<string>(namePath.Split(';'));
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category {Name = name, Parent = parentCategory, Categories = new List<Category>() };
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
            Category category = new Category { Name = name, Parent = parentCategory, Categories = new List<Category>() };
            parentCategory?.Categories.Add(category);
            Cache.Add(namePath, category);
            return category;
        }

        public static List<Category> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static Category GetFromDb(long? id)
        {
            if (id == null) return null;
            if (DbCache.ContainsKey((long) id)) return DbCache[(long) id];
            Category ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Category>(id);

                cnn.Close();
            }
            DbCache.Add((long)id, ret);
            return ret;
        }

        public static void ClearCache()
        {
            DbCache.Clear();
        }

        #endregion

        #endregion
    }
}
