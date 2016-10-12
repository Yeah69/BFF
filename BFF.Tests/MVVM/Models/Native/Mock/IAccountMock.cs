using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.MVVM.Models.Native.Mock
{
    public static class IAccountMock
    {
        public static IList<Mock<IAccount>> AccountMocks => LazyMock.Value;

        public static IList<IAccount> Accounts => Lazy.Value;

        private static readonly Lazy<IList<Mock<IAccount>>> LazyMock = new Lazy<IList<Mock<IAccount>>>(() => new List<Mock<IAccount>>
                                                                                              {
                                                                                                  CreateMock(1, "Bank", 32369),
                                                                                                  CreateMock(2, "Wallet", 6969),
                                                                                                  CreateMock(3, "CreditCard", -2369)
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);
        
        private static readonly Lazy<IList<IAccount>> Lazy = new Lazy<IList<IAccount>>(() => AccountMocks.Select(am => am.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<IAccount> CreateMock(long id, string name, long startingBalance)
        {
            Mock<IAccount> mock = new Mock<IAccount>();

            mock.SetupGet(a => a.Id).Returns(id);
            mock.SetupGet(a => a.Name).Returns(name);
            mock.SetupGet(a => a.StartingBalance).Returns(startingBalance);

            return mock;
        }
    }
}