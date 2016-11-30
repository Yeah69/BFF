using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public class ParentIncomeMoq
    {
        public static IList<IParentIncome> Mocks => new List<IParentIncome>
        {
            CreateMock(1, 2, DateTime.Today - TimeSpan.FromDays(69), 1, "Diner", true),
            CreateMock(2, 2, DateTime.Today - TimeSpan.FromDays(69), 8, "Movie Night", true)
        };

        private static IParentIncome CreateMock(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
        {
            IParentIncome mock = Substitute.For<IParentIncome>();

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