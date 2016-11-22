using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public class ParentTransactionMoq
    {
        public static IList<Mock<IParentTransaction>> Mocks => new List<Mock<IParentTransaction>>
        {
            CreateMock(1, 2, DateTime.Today - TimeSpan.FromDays(69), 1, "Diner", true),
            CreateMock(2, 2, DateTime.Today - TimeSpan.FromDays(69), 8, "Movie Night", true)
        };

        private static Mock<IParentTransaction> CreateMock(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
        {
            Mock<IParentTransaction> mock = new Mock<IParentTransaction>();

            mock.SetupGet(pt => pt.Id).Returns(id);
            mock.SetupGet(pt => pt.AccountId).Returns(accountId);
            mock.SetupGet(pt => pt.Date).Returns(date);
            mock.SetupGet(pt => pt.PayeeId).Returns(payeeId);
            mock.SetupGet(pt => pt.Memo).Returns(memo);
            mock.SetupGet(pt => pt.Cleared).Returns(cleared);

            return mock;
        }
    }
}