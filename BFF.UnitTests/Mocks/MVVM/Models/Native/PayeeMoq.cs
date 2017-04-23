using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class PayeeMoq
    {
        public static IPayee NakedFake => Substitute.For<IPayee>();

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

        public static IPayee NotInserted => CreateMock(-1, "Not Inserted Payee");

        public static IList<IPayee> NotValidToInsert
        {
            get
            {
                IPayee nullName = Substitute.For<IPayee>();
                nullName.Id.Returns(-1);
                nullName.Name.Returns(default(string));
                IPayee emptyName = Substitute.For<IPayee>();
                emptyName.Id.Returns(-1);
                emptyName.Name.Returns("");
                IPayee whitespaceName = Substitute.For<IPayee>();
                whitespaceName.Id.Returns(-1);
                whitespaceName.Name.Returns("    ");
                return new List<IPayee> { nullName, emptyName, whitespaceName };
            }
        }

        private static IPayee CreateMock(long id, string name)
        {
            IPayee payee = Substitute.For<IPayee>();
            payee.Id.Returns(id);
            payee.Name.Returns(name);
            return payee;
        }
    }
}