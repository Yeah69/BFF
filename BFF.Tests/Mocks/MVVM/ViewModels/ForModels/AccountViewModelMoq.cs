using System.Collections.Generic;
using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class AccountViewModelMoq
    {
        public static IList<IAccountViewModel> Mocks => new List<IAccountViewModel>
        {
            CreateMock(1, "Bank", 32369),
            CreateMock(2, "Wallet", 6969),
            CreateMock(3, "CreditCard", -2369)
        };

        private static IAccountViewModel CreateMock(long id, string name, long startingBalance)
        {
            IAccountViewModel mock = Substitute.For<IAccountViewModel>();

            mock.Id.Returns(id);
            mock.Name.Returns(name);
            mock.StartingBalance.Returns(startingBalance);

            return mock;
        }
    }
}