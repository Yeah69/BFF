using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using NSubstitute;
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
                IBffOrm ormMock = BffOrmMoq.Mock;

                //Act
                summaryAccount.Insert(ormMock);
                summaryAccount.Update(ormMock);
                summaryAccount.Delete(ormMock);

                //Assert
                //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not interact with the ORM.
                ormMock.DidNotReceive().Insert(Arg.Any<SummaryAccount>());
                ormMock.DidNotReceive().Update(Arg.Any<SummaryAccount>());
                ormMock.DidNotReceive().Delete(Arg.Any<SummaryAccount>());
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