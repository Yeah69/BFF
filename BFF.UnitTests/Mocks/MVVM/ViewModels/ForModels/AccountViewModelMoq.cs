using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class AccountViewModelMoq
    {
        public static IAccountViewModel Naked => Substitute.For<IAccountViewModel>();
    }
}