using System;
using System.Threading;
using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.DB
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

            mock.Setup(orm => orm.CommonPropertyProvider).Returns(CommonPropertyProviderMoq.CommonPropertyProvider);

            return mock;
        }
    }
}