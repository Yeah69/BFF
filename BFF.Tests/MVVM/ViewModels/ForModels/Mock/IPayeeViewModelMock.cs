using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.MVVM.ViewModels.ForModels.Mock
{
    public static class IPayeeViewModelMock
    {
        public static IList<Mock<IPayeeViewModel>> PayeeViewModelMocks => LazyMock.Value;

        public static IList<IPayeeViewModel> PayeeViewModels => Lazy.Value;

        private static readonly Lazy<IList<Mock<IPayeeViewModel>>> LazyMock = new Lazy<IList<Mock<IPayeeViewModel>>>(() => new List<Mock<IPayeeViewModel>>
                                                                                              {
                                                                                                  CreateMock(1, "Tony's Pizza"),
                                                                                                  CreateMock(2, "Mother"),
                                                                                                  CreateMock(3, "cineplex"),
                                                                                                  CreateMock(4, "Work")
                                                                                              }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IList<IPayeeViewModel>> Lazy = new Lazy<IList<IPayeeViewModel>>(() => PayeeViewModelMocks.Select(pm => pm.Object).ToList(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<IPayeeViewModel> CreateMock(long id, string name)
        {
            Mock<IPayeeViewModel> mock = new Mock<IPayeeViewModel>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.Name).Returns(name);

            return mock;
        }
    }
}