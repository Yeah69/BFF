using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.MVVM.Models.Native.Mock
{
    public static class PayeeMoq
    {
        public static IList<Mock<IPayee>> PayeeMocks => LazyMock.Value;

        public static IList<IPayee> Payees => Lazy.Value;

        private static readonly Lazy<IList<Mock<IPayee>>> LazyMock = new Lazy<IList<Mock<IPayee>>>(() => new List<Mock<IPayee>>
                                                                                              {
                                                                                                  CreateMock(1, "Tony's Pizza"),
                                                                                                  CreateMock(2, "Mother"),
                                                                                                  CreateMock(3, "cineplex"),
                                                                                                  CreateMock(4, "Work")
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IList<IPayee>> Lazy = new Lazy<IList<IPayee>>(() => PayeeMocks.Select(pm => pm.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<IPayee> CreateMock(long id, string name)
        {
            Mock<IPayee> mock = new Mock<IPayee>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.Name).Returns(name);

            return mock;
        }
    }
}