using System.Collections.Generic;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class PayeeViewModelMoq
    {
        public static IList<Mock<IPayeeViewModel>> PayeeViewModelMocks => new List<Mock<IPayeeViewModel>>
        {
            CreateMock(1, "Tony's Pizza"),
            CreateMock(2, "Mother"),
            CreateMock(3, "cineplex"),
            CreateMock(4, "Work")
        };

        private static Mock<IPayeeViewModel> CreateMock(long id, string name)
        {
            Mock<IPayeeViewModel> mock = new Mock<IPayeeViewModel>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.Name).Returns(name);

            return mock;
        }
    }
}