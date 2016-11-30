using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class PayeeMoq
    {
        public static IList<IPayee> Mocks => new List<IPayee>
        {
            CreateMock(1, "Tony's Pizza"),
            CreateMock(2, "Mother"),
            CreateMock(3, "cineplex"),
            CreateMock(4, "Work"),
            CreateMock(5, "amazon"),
            CreateMock(6, "Walmart"),
            CreateMock(7, "Mensa"),
            CreateMock(8, "BFF")
        };

        private static IPayee CreateMock(long id, string name)
        {
            IPayee payee = Substitute.For<IPayee>();
            payee.Id.Returns(id);
            payee.Name.Returns(name);
            return payee;
        }
    }
}