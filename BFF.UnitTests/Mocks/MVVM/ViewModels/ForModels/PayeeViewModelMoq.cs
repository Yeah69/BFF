using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class PayeeViewModelMoq
    {
        public static IPayeeViewModel Naked => Substitute.For<IPayeeViewModel>();
    }
}