using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class ParentTransactionMoq
    {
        public static IList<IParentTransaction> Mocks => new List<IParentTransaction>
        {
            CreateMock(1, 2, new DateTime(1969, 6, 9), 1, "Diner", true),
            CreateMock(2, 2, new DateTime(1969, 6, 11), 8, "Movie Night", true)
        };

        private static IParentTransaction CreateMock(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
        {
            IParentTransaction mock = Substitute.For<IParentTransaction>();

            mock.Id.Returns(id);
            mock.AccountId.Returns(accountId);
            mock.Date.Returns(date);
            mock.PayeeId.Returns(payeeId);
            mock.Memo.Returns(memo);
            mock.Cleared.Returns(cleared);

            return mock;
        }
    }
}