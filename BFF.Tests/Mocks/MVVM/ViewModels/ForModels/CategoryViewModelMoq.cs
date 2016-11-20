using System.Collections.Generic;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class CategoryViewModelMoq
    {
        private struct CategoryData
        {
            public long Id;
            public string Name;
            public long? ParentId;
        }

        private static readonly CategoryData[] CategorySet =
        {
            new CategoryData {Id = 1, Name= "Food", ParentId = null},
            new CategoryData {Id = 2, Name= "Outside", ParentId = 1},
            new CategoryData {Id = 3, Name= "Restaurants", ParentId = 2},
            new CategoryData {Id = 4, Name= "Mensa", ParentId = 2},
            new CategoryData {Id = 5, Name= "Income", ParentId = null},
            new CategoryData {Id = 6, Name= "Entertainment", ParentId = 5},
            new CategoryData {Id = 7, Name= "Movie in Cinema", ParentId = null},
            new CategoryData {Id = 8, Name= "Little Things", ParentId = 7},
            new CategoryData {Id = 9, Name= "Sweets/Snacks", ParentId = null},
            new CategoryData {Id = 10, Name= "Salary", ParentId = 9},
            new CategoryData {Id = 11, Name= "Outside", ParentId = 5},
            new CategoryData {Id = 12, Name= "Bonus", ParentId = 5}
        };

        public static IList<Mock<ICategoryViewModel>> Mocks => new List<Mock<ICategoryViewModel>>
        {
            CreateMock(CategorySet[0].Id, CategorySet[0].Name,  CategorySet[0].ParentId),
            CreateMock(CategorySet[1].Id, CategorySet[1].Name,  CategorySet[1].ParentId),
            CreateMock(CategorySet[2].Id, CategorySet[2].Name,  CategorySet[2].ParentId),
            CreateMock(CategorySet[3].Id, CategorySet[3].Name,  CategorySet[3].ParentId),
            CreateMock(CategorySet[4].Id, CategorySet[4].Name,  CategorySet[4].ParentId),
            CreateMock(CategorySet[5].Id, CategorySet[5].Name,  CategorySet[5].ParentId),
            CreateMock(CategorySet[6].Id, CategorySet[6].Name,  CategorySet[6].ParentId),
            CreateMock(CategorySet[7].Id, CategorySet[7].Name,  CategorySet[7].ParentId),
            CreateMock(CategorySet[8].Id, CategorySet[8].Name,  CategorySet[8].ParentId),
            CreateMock(CategorySet[9].Id, CategorySet[9].Name,  CategorySet[9].ParentId),
            CreateMock(CategorySet[10].Id, CategorySet[10].Name,  CategorySet[10].ParentId),
            CreateMock(CategorySet[11].Id, CategorySet[11].Name,  CategorySet[11].ParentId)
        };

        private static Mock<ICategoryViewModel> CreateMock(long id, string name, long? parentId)
        {
            Mock<ICategoryViewModel> mock = new Mock<ICategoryViewModel>();

            mock.SetupGet(c => c.Id).Returns(id);
            mock.SetupGet(c => c.Name).Returns(name);
            if(parentId != null)
                mock.SetupGet(c => c.Parent).Returns(CreateMock(CategorySet[(int)parentId].Id, CategorySet[(int)parentId].Name, CategorySet[(int)parentId - 1].ParentId).Object);
            else
                mock.SetupGet(c => c.Parent).Returns(() => null);

            return mock;
        }
    }
}