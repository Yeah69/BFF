using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.MVVM.ViewModels.ForModels.Mock
{
    public static class ICategoryViewModelMock
    {
        private struct CategoryData
        {
            public long Id;
            public string Name;
            public long? ParentId;
        }

        private static CategoryData[] categoryData =
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

        public static IList<Mock<ICategoryViewModel>> CategoryViewModelMocks => LazyMock.Value;

        public static IList<ICategoryViewModel> CategorieViewModels => Lazy.Value;

        private static readonly Lazy<IList<Mock<ICategoryViewModel>>> LazyMock = new Lazy<IList<Mock<ICategoryViewModel>>>(() => new List<Mock<ICategoryViewModel>>
                                                                                              {
                                                                                                  CreateMock(categoryData[0].Id, categoryData[0].Name,  categoryData[0].ParentId),
                                                                                                  CreateMock(categoryData[1].Id, categoryData[1].Name,  categoryData[1].ParentId),
                                                                                                  CreateMock(categoryData[2].Id, categoryData[2].Name,  categoryData[2].ParentId),
                                                                                                  CreateMock(categoryData[3].Id, categoryData[3].Name,  categoryData[3].ParentId),
                                                                                                  CreateMock(categoryData[4].Id, categoryData[4].Name,  categoryData[4].ParentId),
                                                                                                  CreateMock(categoryData[5].Id, categoryData[5].Name,  categoryData[5].ParentId),
                                                                                                  CreateMock(categoryData[6].Id, categoryData[6].Name,  categoryData[6].ParentId),
                                                                                                  CreateMock(categoryData[7].Id, categoryData[7].Name,  categoryData[7].ParentId),
                                                                                                  CreateMock(categoryData[8].Id, categoryData[8].Name,  categoryData[8].ParentId),
                                                                                                  CreateMock(categoryData[9].Id, categoryData[9].Name,  categoryData[9].ParentId),
                                                                                                  CreateMock(categoryData[10].Id, categoryData[10].Name,  categoryData[10].ParentId),
                                                                                                  CreateMock(categoryData[11].Id, categoryData[11].Name,  categoryData[11].ParentId)
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IList<ICategoryViewModel>> Lazy = new Lazy<IList<ICategoryViewModel>>(() => CategoryViewModelMocks.Select(pm => pm.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<ICategoryViewModel> CreateMock(long id, string name, long? parentId)
        {
            Mock<ICategoryViewModel> mock = new Mock<ICategoryViewModel>();

            mock.SetupGet(c => c.Id).Returns(id);
            mock.SetupGet(c => c.Name).Returns(name);
            if(parentId != null)
                mock.SetupGet(c => c.Parent).Returns(CreateMock(categoryData[(int)parentId].Id, categoryData[(int)parentId].Name, categoryData[(int)parentId - 1].ParentId).Object);
            else
                mock.SetupGet(c => c.Parent).Returns(() => null);

            return mock;
        }
    }
}