using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static Mock<IBffOrm> Mock => CreateMock();

        internal static Mock<IBffOrm> CreateMock(Mock<ICommonPropertyProvider> commonPropertyProviderMock = null, 
            IList<Mock<ISubTransaction>> subTransationMocks = null, IList<Mock<ISubIncome>> subIncomeMocks = null)
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

            if (subTransationMocks != null)
            {
                IEnumerable<long> parentIds = subTransationMocks.Select(stm => stm.Object.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.Setup(orm => orm.GetSubTransInc<SubTransaction>(parentId)).
                        Returns(() => subTransationMocks.Where(stm => stm.Object.ParentId == parentId).Select(stm => stm.Object));
                }
            }

            if (subIncomeMocks != null)
            {
                IEnumerable<long> parentIds = subIncomeMocks.Select(stm => stm.Object.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.Setup(orm => orm.GetSubTransInc<SubIncome>(parentId)).
                        Returns(() => subIncomeMocks.Where(stm => stm.Object.ParentId == parentId).Select(stm => stm.Object));
                }
            }

            return mock;
        }
    }
}