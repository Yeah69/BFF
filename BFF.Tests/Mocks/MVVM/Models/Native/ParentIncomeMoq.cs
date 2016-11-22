using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public class ParentIncomeMoq
    {
        public static IList<Mock<IParentIncome>> Mocks => new List<Mock<IParentIncome>>
        {
            CreateMock(1, 2, DateTime.Today - TimeSpan.FromDays(69), 1, "Diner", true),
            CreateMock(2, 2, DateTime.Today - TimeSpan.FromDays(69), 8, "Movie Night", true)
        };

        private static Mock<IParentIncome> CreateMock(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
        {
            Mock<IParentIncome> mock = new Mock<IParentIncome>();

            mock.SetupGet(pi => pi.Id).Returns(id);
            mock.SetupGet(pi => pi.AccountId).Returns(accountId);
            mock.SetupGet(pi => pi.Date).Returns(date);
            mock.SetupGet(pi => pi.PayeeId).Returns(payeeId);
            mock.SetupGet(pi => pi.Memo).Returns(memo);
            mock.SetupGet(pi => pi.Cleared).Returns(cleared);

            return mock;
        }
    }
}