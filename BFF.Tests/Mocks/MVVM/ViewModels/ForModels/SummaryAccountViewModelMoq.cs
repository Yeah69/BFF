using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class SummaryAccountViewModelMoq
    {
        public static IMock<ISummaryAccountViewModel> SummaryAccountViewModelMock = CreateMock();

        public static ISummaryAccountViewModel SummaryAccountViewModel = SummaryAccountViewModelMock.Object;

        private static Mock<ISummaryAccountViewModel> CreateMock()
        {
            Mock<ISummaryAccountViewModel> mock = new Mock<ISummaryAccountViewModel>();

            mock.Setup(savm => savm.RefreshStartingBalance()).Verifiable();

            return mock;
        }
    }
}