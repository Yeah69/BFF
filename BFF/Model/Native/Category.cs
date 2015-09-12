﻿using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Category : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement => $@"CREATE TABLE [{nameof(Category)}s](
                        {nameof(ID)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100));";

        [Key]
        public override long ID { get; set; } = -1;

        public string Name { get; set; }

        [Write(false)]
        public List<Category> Categories { get; set; }

        [Write(false)]
        public Category ParentCategory { get; set; }

        #endregion

        #region Methods

        

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
            Stack<string> nameStack = new Stack<string>(namePath.Split(';'));
            string name = nameStack.Pop();
            Category parentCategory = GetOrCreate(nameStack);
            Category category = new Category {Name = name, ParentCategory = parentCategory, Categories = new List<Category>() };
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
            Category category = new Category { Name = name, ParentCategory = parentCategory, Categories = new List<Category>() };
            parentCategory?.Categories.Add(category);
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
