using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class SummaryAccountViewModelMoq
    {
        public static ISummaryAccountViewModel Mock => CreateMock();

        private static ISummaryAccountViewModel CreateMock()
        {
            ISummaryAccountViewModel mock = Substitute.For<ISummaryAccountViewModel>();

            return mock;
        }
    }
}