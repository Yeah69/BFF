using System.Collections.Generic;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class AccountViewModelMoq
    {
        public static IList<Mock<IAccountViewModel>> AccountViewModelMocks => new List<Mock<IAccountViewModel>>
        {
            CreateMock(1, "Bank", 32369),
            CreateMock(2, "Wallet", 6969),
            CreateMock(3, "CreditCard", -2369)
        };

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