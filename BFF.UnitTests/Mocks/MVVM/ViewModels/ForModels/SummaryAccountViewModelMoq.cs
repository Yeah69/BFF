using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class SummaryAccountViewModelMoq
    {
        public static ISummaryAccountViewModel Naked => Substitute.For<ISummaryAccountViewModel>();
    }
}