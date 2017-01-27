using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubTransactionMoq
    {
        public static IList<ISubTransaction> Mocks => new List<ISubTransaction>
        {
            CreateMock(1, 1, 3, "Tony's Pizza", -450),
            CreateMock(2, 1, 1, "Something to drink", -150),
            CreateMock(3, 2, 10, "Chips", -120),
            CreateMock(4, 2, 9, "3 4 Bier Normal", -300),
            CreateMock(5, 2, 9, "Rented a movie", -200)
        };

        private static ISubTransaction CreateMock(long id, long parentId, long categoryId, string memo, long sum)
        {
            ISubTransaction mock = Substitute.For<ISubTransaction>();

            mock.Id.Returns(id);
            mock.ParentId.Returns(parentId);
            mock.CategoryId.Returns(categoryId);
            mock.Memo.Returns(memo);
            mock.Sum.Returns(sum);

            return mock;
        }
    }
}