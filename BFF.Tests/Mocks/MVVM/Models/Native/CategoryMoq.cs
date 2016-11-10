using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class CategoryMoq
    {

        public static IList<Mock<ICategory>> CategoryMocks => LazyMock.Value;

        public static IList<ICategory> Categories => Lazy.Value;

        private static readonly Lazy<IList<Mock<ICategory>>> LazyMock = new Lazy<IList<Mock<ICategory>>>(() => new List<Mock<ICategory>>
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
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IList<ICategory>> Lazy = new Lazy<IList<ICategory>>(() => CategoryMocks.Select(pm => pm.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<ICategory> CreateMock(long id, string name, long? parentId)
        {
            Mock<ICategory> mock = new Mock<ICategory>();

            mock.SetupGet(c => c.Id).Returns(id);
            mock.SetupGet(c => c.Name).Returns(name);
            mock.SetupGet(c => c.ParentId).Returns(parentId);

            return mock;
        }
    }
}