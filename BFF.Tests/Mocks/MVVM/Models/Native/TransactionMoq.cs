using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransactionMoq
    {
        public static IList<Mock<ITransaction>> Mocks => new List<Mock<ITransaction>>
        {
            CreateMock(1, 2, DateTime.Today - TimeSpan.FromDays(69), 2, 6, "Jewelry", -8000, true),
            CreateMock(2, 1, DateTime.Today - TimeSpan.FromDays(67), 5, 14, "C# in Depth", -3500, true),
            CreateMock(3, 3, DateTime.Today - TimeSpan.FromDays(65), 7, 2, "Lunch", -350, true)
        };

        private static Mock<ITransaction> CreateMock(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
        {
            Mock<ITransaction> mock = new Mock<ITransaction>();

            mock.SetupGet(t => t.Id).Returns(id);
            mock.SetupGet(t => t.AccountId).Returns(accountId);
            mock.SetupGet(t => t.Date).Returns(date);
            mock.SetupGet(t => t.PayeeId).Returns(payeeId);
            mock.SetupGet(t => t.CategoryId).Returns(categoryId);
            mock.SetupGet(t => t.Memo).Returns(memo);
            mock.SetupGet(t => t.Sum).Returns(sum);
            mock.SetupGet(t => t.Cleared).Returns(cleared);

            return mock;
        }
    }
}