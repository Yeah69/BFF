using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransactionMoq
    {
        public static IList<ITransaction> Mocks => new List<ITransaction>
        {
            CreateMock(1, 2, new DateTime(1969, 6, 9), 2, 6, "Jewelry", -8000, true),
            CreateMock(2, 1, new DateTime(1969, 6, 11), 5, 14, "C# in Depth", -3500, true),
            CreateMock(3, 3, new DateTime(1969, 6, 12), 7, 2, "Lunch", -350, true)
        };

        private static ITransaction CreateMock(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
        {
            ITransaction mock = Substitute.For<ITransaction>();

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