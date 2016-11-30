using System.Collections.Generic;
using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class PayeeViewModelMoq
    {
        public static IList<IPayeeViewModel> Mocks => new List<IPayeeViewModel>
        {
            CreateMock(1, "Tony's Pizza"),
            CreateMock(2, "Mother"),
            CreateMock(3, "cineplex"),
            CreateMock(4, "Work")
        };

        private static IPayeeViewModel CreateMock(long id, string name)
        {
            IPayeeViewModel mock = Substitute.For<IPayeeViewModel>();

            mock.Id.Returns(id);
            mock.Name.Returns(name);

            return mock;
        }
    }
}