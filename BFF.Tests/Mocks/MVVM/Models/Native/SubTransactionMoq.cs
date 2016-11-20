using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubTransactionMoq
    {
        public static IList<Mock<ISubTransaction>> Mocks => new List<Mock<ISubTransaction>>
        {
            CreateMock(1, 1, 3, "Tony's Pizza", -450),
            CreateMock(2, 1, 1, "Something to drink", -150),
            CreateMock(3, 2, 10, "Chips", -120),
            CreateMock(4, 2, 9, "3 4 Bier Normal", -300),
            CreateMock(5, 2, 9, "Rented a movie", -200)
        };

        private static Mock<ISubTransaction> CreateMock(long id, long parentId, long categoryId, string memo, long sum)
        {
            Mock<ISubTransaction> mock = new Mock<ISubTransaction>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.ParentId).Returns(parentId);
            mock.SetupGet(p => p.CategoryId).Returns(categoryId);
            mock.SetupGet(p => p.Memo).Returns(memo);
            mock.SetupGet(p => p.Sum).Returns(sum);

            return mock;
        }
    }
}