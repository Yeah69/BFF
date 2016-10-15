using System;
using System.Threading;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.MVVM.ViewModels.ForModels.Mock;
using Moq;

namespace BFF.Tests.DB.Mock
{
    public static class BffOrmMoq
    {
        public static Mock<IBffOrm> BffOrmMock => LazyMock.Value;

        public static IBffOrm BffOrm => Lazy.Value;

        private static readonly Lazy<Mock<IBffOrm>> LazyMock = new Lazy<Mock<IBffOrm>>(CreateMock, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IBffOrm> Lazy = new Lazy<IBffOrm>(() => BffOrmMock.Object, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Mock<IBffOrm> CreateMock()
        {
            Mock<IBffOrm> mock = new Mock<IBffOrm>();

            mock.Setup(orm => orm.Insert(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<IAccount>())).Verifiable();

            mock.Setup(orm => orm.Insert(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<ICategory>())).Verifiable();

            mock.Setup(orm => orm.Insert(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<IPayee>())).Verifiable();

            foreach(IAccountViewModel accountViewModel in AccountViewModelMoq.AccountViewModels)
            {
                mock.Setup(orm => orm.CommonPropertyProvider.GetAccountViewModel(accountViewModel.Id)).Returns(accountViewModel);
            }

            foreach(ICategoryViewModel categoryViewModel in CategoryViewModelMoq.CategorieViewModels)
            {
                mock.Setup(orm => orm.CommonPropertyProvider.GetCategoryViewModel(categoryViewModel.Id)).Returns(categoryViewModel);
            }

            foreach(IPayeeViewModel payeeViewModel in PayeeViewModelMoq.PayeeViewModels)
            {
                mock.Setup(orm => orm.CommonPropertyProvider.GetPayeeViewModel(payeeViewModel.Id)).Returns(payeeViewModel);
            }

            return mock;
        }
    }
}