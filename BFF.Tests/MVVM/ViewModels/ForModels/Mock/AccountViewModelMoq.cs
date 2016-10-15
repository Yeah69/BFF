using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.MVVM.ViewModels.ForModels.Mock
{
    public static class AccountViewModelMoq
    {
        public static IList<Mock<IAccountViewModel>> AccountViewModelMocks => LazyMock.Value;

        public static IList<IAccountViewModel> AccountViewModels => Lazy.Value;

        private static readonly Lazy<IList<Mock<IAccountViewModel>>> LazyMock = new Lazy<IList<Mock<IAccountViewModel>>>(() => new List<Mock<IAccountViewModel>>
                                                                                              {
                                                                                                  CreateMock(1, "Bank", 32369),
                                                                                                  CreateMock(2, "Wallet", 6969),
                                                                                                  CreateMock(3, "CreditCard", -2369)
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IList<IAccountViewModel>> Lazy = new Lazy<IList<IAccountViewModel>>(() => AccountViewModelMocks.Select(am => am.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<IAccountViewModel> CreateMock(long id, string name, long startingBalance)
        {
            Mock<IAccountViewModel> mock = new Mock<IAccountViewModel>();

            mock.SetupGet(a => a.Id).Returns(id);
            mock.SetupGet(a => a.Name).Returns(name);
            mock.SetupGet(a => a.StartingBalance).Returns(startingBalance);

            return mock;
        }
    }
}