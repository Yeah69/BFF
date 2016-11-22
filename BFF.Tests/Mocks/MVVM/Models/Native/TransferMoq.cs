using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransferMoq
    {
        public static IList<Mock<ITransfer>> Mocks => new List<Mock<ITransfer>>
        {
            CreateMock(1, 1, 2, DateTime.Today - TimeSpan.FromDays(69), "Cash", 10000, false),
            CreateMock(2, 1, 3, DateTime.Today - TimeSpan.FromDays(67), "Charge CreditCard", 20000, true)
        };

        private static Mock<ITransfer> CreateMock(long id, long fromAccountId, long toAccountId, DateTime date, string memo, long sum, bool cleared)
        {
            Mock<ITransfer> mock = new Mock<ITransfer>();

            mock.SetupGet(t => t.Id).Returns(id);
            mock.SetupGet(t => t.FromAccountId).Returns(fromAccountId);
            mock.SetupGet(t => t.ToAccountId).Returns(toAccountId);
            mock.SetupGet(t => t.Date).Returns(date);
            mock.SetupGet(t => t.Memo).Returns(memo);
            mock.SetupGet(t => t.Sum).Returns(sum);
            mock.SetupGet(t => t.Cleared).Returns(cleared);

            return mock;
        }
    }
}