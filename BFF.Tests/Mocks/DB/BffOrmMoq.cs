using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static Mock<IBffOrm> BffOrmMock => CreateMock();

        internal static Mock<IBffOrm> CreateMock(Mock<ICommonPropertyProvider> commonPropertyProviderMock = null)
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

            if(commonPropertyProviderMock != null)
                mock.Setup(orm => orm.CommonPropertyProvider).Returns(commonPropertyProviderMock.Object);

            return mock;
        }
    }
}