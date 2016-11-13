using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class PayeeMoq
    {
        public static IList<Mock<IPayee>> PayeeMocks => new List<Mock<IPayee>>
        {
            CreateMock(1, "Tony's Pizza"),
            CreateMock(2, "Mother"),
            CreateMock(3, "cineplex"),
            CreateMock(4, "Work")
        };

        private static Mock<IPayee> CreateMock(long id, string name)
        {
            Mock<IPayee> mock = new Mock<IPayee>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.Name).Returns(name);

            return mock;
        }
    }
}