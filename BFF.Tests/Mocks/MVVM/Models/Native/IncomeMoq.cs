using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class IncomeMoq
    {
        public static IList<IIncome> Mocks => new List<IIncome>
        {
            CreateMock(1, 2, new DateTime(1969, 6, 9), 8, 16, "Debt payeed back", 2000, true),
            CreateMock(2, 1, new DateTime(1969, 6, 11), 4, 11, "Salary for June", 120000, true)
        };

        private static IIncome CreateMock(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
        {
            IIncome mock = Substitute.For<IIncome>();

            mock.Id.Returns(id);
            mock.AccountId.Returns(accountId);
            mock.Date.Returns(date);
            mock.PayeeId.Returns(payeeId);
            mock.CategoryId.Returns(categoryId);
            mock.Memo.Returns(memo);
            mock.Sum.Returns(sum);
            mock.Cleared.Returns(cleared);

            return mock;
        }
    }
}