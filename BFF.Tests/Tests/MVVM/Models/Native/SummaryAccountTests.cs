using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using Moq;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class SummaryAccountTests
    {
        public class ConstructionTests
        {
            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                SummaryAccount summaryAccount = new SummaryAccount();

                //Act

                //Assert
                Assert.Equal(-1L, summaryAccount.Id);
                Assert.Equal(0L, summaryAccount.StartingBalance);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                SummaryAccount summaryAccount = new SummaryAccount();
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                summaryAccount.Insert(ormMock.Object);
                summaryAccount.Update(ormMock.Object);
                summaryAccount.Delete(ormMock.Object);

                //Assert
                //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not interact with the ORM.
                ormMock.Verify(orm => orm.Insert(It.IsAny<SummaryAccount>()), Times.Never);
                ormMock.Verify(orm => orm.Update(It.IsAny<SummaryAccount>()), Times.Never);
                ormMock.Verify(orm => orm.Delete(It.IsAny<SummaryAccount>()), Times.Never);
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                SummaryAccount summaryAccount = new SummaryAccount();

                //Act + Assert
                //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not throw an exception if the given ORM is not valid.
                summaryAccount.Insert(null);
                summaryAccount.Update(null);
                summaryAccount.Delete(null);
            }
        }
    }
}