using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class IncomeMoq
    {
        public static IList<Mock<IIncome>> Mocks => new List<Mock<IIncome>>
        {
            CreateMock(1, 2, DateTime.Today - TimeSpan.FromDays(69), 8, 16, "Debt payeed back", 2000, true),
            CreateMock(2, 1, DateTime.Today - TimeSpan.FromDays(67), 4, 11, "Salary for June", 120000, true)
        };

        private static Mock<IIncome> CreateMock(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
        {
            Mock<IIncome> mock = new Mock<IIncome>();

            mock.SetupGet(i => i.Id).Returns(id);
            mock.SetupGet(i => i.AccountId).Returns(accountId);
            mock.SetupGet(i => i.Date).Returns(date);
            mock.SetupGet(i => i.PayeeId).Returns(payeeId);
            mock.SetupGet(i => i.CategoryId).Returns(categoryId);
            mock.SetupGet(i => i.Memo).Returns(memo);
            mock.SetupGet(i => i.Sum).Returns(sum);
            mock.SetupGet(i => i.Cleared).Returns(cleared);

            return mock;
        }
    }
}