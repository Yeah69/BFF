using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class CategoryMoq
    {

        public static IList<ICategory> Mocks => new List<ICategory>
        {
            CreateMock(1, "Food", null),
            CreateMock(2, "Outside", 1),
            CreateMock(3, "Restaurants", 2),
            CreateMock(4, "Mensa", 2),
            CreateMock(5, "Income", null),
            CreateMock(6, "Birthday", 5),
            CreateMock(7, "Entertainment", null),
            CreateMock(8, "Movie in Cinema", 7),
            CreateMock(9, "Little Things", null),
            CreateMock(10, "Sweets/Snacks", 9),
            CreateMock(11, "Salary", 5),
            CreateMock(12, "Bonus", 5),
            CreateMock(13, "Education", null),
            CreateMock(14, "Books", 13),
            CreateMock(15, "Grocery", null),
            CreateMock(16, "Debt", null)
        };

        private static ICategory CreateMock(long id, string name, long? parentId)
        {
            ICategory category = Substitute.For<BFF.MVVM.Models.Native.ICategory>();
            category.Id.Returns(id);
            category.Name.Returns(name);
            category.ParentId.Returns(parentId);
            return category;
        }
    }
}