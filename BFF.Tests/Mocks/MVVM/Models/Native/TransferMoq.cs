using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransferMoq
    {
        public static IList<ITransfer> Mocks => new List<ITransfer>
        {
            CreateMock(1, 1, 2, DateTime.Today - TimeSpan.FromDays(69), "Cash", 10000, false),
            CreateMock(2, 1, 3, DateTime.Today - TimeSpan.FromDays(67), "Charge CreditCard", 20000, true)
        };

        private static ITransfer CreateMock(long id, long fromAccountId, long toAccountId, DateTime date, string memo, long sum, bool cleared)
        {
            ITransfer mock = Substitute.For<ITransfer>();

            mock.Id.Returns(id);
            mock.FromAccountId.Returns(fromAccountId);
            mock.ToAccountId.Returns(toAccountId);
            mock.Date.Returns(date);
            mock.Memo.Returns(memo);
            mock.Sum.Returns(sum);
            mock.Cleared.Returns(cleared);

            return mock;
        }
    }
}