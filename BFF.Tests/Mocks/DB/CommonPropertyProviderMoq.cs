using System;
using System.Threading;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.DB
{
    public static class CommonPropertyProviderMoq
    {
        public static Mock<ICommonPropertyProvider> CommonPropertyProviderMock => LazyMock.Value;

        public static ICommonPropertyProvider CommonPropertyProvider => Lazy.Value;

        private static readonly Lazy<Mock<ICommonPropertyProvider>> LazyMock = new Lazy<Mock<ICommonPropertyProvider>>(CreateMock, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<ICommonPropertyProvider> Lazy = new Lazy<ICommonPropertyProvider>(() => CommonPropertyProviderMock.Object, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<ICommonPropertyProvider> CreateMock()
        {
            Mock<ICommonPropertyProvider> mock = new Mock<ICommonPropertyProvider>();

            mock.Setup(cpp => cpp.Add(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<IAccount>())).Verifiable();

            mock.Setup(cpp => cpp.Add(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<ICategory>())).Verifiable();

            mock.Setup(cpp => cpp.Add(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<IPayee>())).Verifiable();

            foreach (IAccountViewModel accountViewModel in AccountViewModelMoq.AccountViewModels)
            {
                mock.Setup(cpp => cpp.GetAccountViewModel(accountViewModel.Id)).Returns(accountViewModel);
            }

            foreach (ICategoryViewModel categoryViewModel in CategoryViewModelMoq.CategorieViewModels)
            {
                mock.Setup(cpp => cpp.GetCategoryViewModel(categoryViewModel.Id)).Returns(categoryViewModel);
            }

            foreach (IPayeeViewModel payeeViewModel in PayeeViewModelMoq.PayeeViewModels)
            {
                mock.Setup(cpp => cpp.GetPayeeViewModel(payeeViewModel.Id)).Returns(payeeViewModel);
            }

            mock.SetupGet(cpp => cpp.SummaryAccountViewModel).Returns(SummaryAccountViewModelMoq.SummaryAccountViewModel);

            return mock;
        }
    }
}
