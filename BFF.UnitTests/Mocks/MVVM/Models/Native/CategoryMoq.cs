using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class CategoryMoq
    {
        internal static readonly (long Id, string Name, long? ParentId)[] CategorySet =
        {
            (1, "Food", null),
            (2, "Outside", 1),
            (3, "Restaurants", 2),
            (4, "Mensa", 2),
            (5, "Income", null),
            (6, "Birthday", 5),
            (7, "Entertainment", null),
            (8, "Movie in Cinema", 7),
            (9, "Little Things", null),
            (10, "Sweets/Snacks", 9),
            (11, "Salary", 5),
            (12, "Bonus", 5),
            (13, "Education", null),
            (14, "Books", 13),
            (15, "Grocery", null),
            (16, "Debt", null)
        };

        public static IList<ICategory> Mocks
        {
            get
            {
                IList<ICategory> mocks = new List<ICategory>();
                foreach(var categorySetTuple in CategorySet)
                {
                    mocks.Add(CreateMock(categorySetTuple.Id,
                                         categorySetTuple.Name,
                                         categorySetTuple.ParentId));
                }
                return mocks;
            }
        }

        public static IList<ICategory> NotInserted
        {
            get
            {
                IList<ICategory> ret = new List<ICategory>();
                ret.Add(CreateMock(-1, "Not Inserted Category", null));
                foreach(ICategory category in Mocks)
                {
                    ret.Add(CreateMock(-1, "Not Inserted Category", category.Id));
                }
                return ret;
            }
        }

        public static IList<ICategory> NotValidToInsert
        {
            get
            {
                ICategory nullName = Substitute.For<ICategory>();
                nullName.Id.Returns(-1);
                nullName.Name.Returns(default(string));
                nullName.ParentId.Returns(default(long?));
                ICategory emptyName = Substitute.For<ICategory>();
                emptyName.Id.Returns(-1);
                emptyName.Name.Returns("");
                emptyName.ParentId.Returns(default(long?));
                ICategory whitespaceName = Substitute.For<ICategory>();
                whitespaceName.Id.Returns(-1);
                whitespaceName.Name.Returns("    ");
                whitespaceName.ParentId.Returns(default(long?));
                IList<ICategory> ret = new List<ICategory>{ nullName, emptyName, whitespaceName};
                foreach(ICategory category in Mocks)
                {
                    ret.Add(CreateMock(-1, category.Name, category.ParentId));
                }
                return ret;
            }
        }

        private static ICategory CreateMock(long id, string name, long? parentId)
        {
            ICategory category = Substitute.For<ICategory>();
            category.Id.Returns(id);
            category.Name.Returns(name);
            category.ParentId.Returns(parentId);
            return category;
        }
    }
}