using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class SummaryAccountTests
    {
        SummaryAccount SummaryAccountFactory => new SummaryAccount();

        private long IdDefaultVaule => -1L;

        private long StartingBalanceDefaultValue => 0L;

        [Fact]
        public void Ctor_Nothing_HasDefaultId()
        {
            //Arrange
            SummaryAccount summaryAccount = new SummaryAccount();

            //Act

            //Assert
            Assert.Equal(IdDefaultVaule, summaryAccount.Id);
        }

        [Fact]
        public void Ctor_Nothing_HasDefaultStartingBalance()
        {
            //Arrange
            SummaryAccount summaryAccount = new SummaryAccount();

            //Act

            //Assert
            Assert.Equal(StartingBalanceDefaultValue, summaryAccount.StartingBalance);
        }

        [Fact]
        public void Insert_Orm_DoesNotCallInsertOnOrm()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;
            IBffOrm ormMock = BffOrmMoq.NakedFake;

            //Act
            summaryAccount.Insert(ormMock);

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not interact with the ORM.
            ormMock.DidNotReceive().Insert(Arg.Any<SummaryAccount>());
        }

        [Fact]
        public void Insert_Null_DoesNothing()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;

            //Act 

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not throw an exception if the given ORM is not valid.
            summaryAccount.Insert(null);
        }

        [Fact]
        public void Update_Orm_DoesNotCallUpdateOnOrm()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;
            IBffOrm ormMock = BffOrmMoq.NakedFake;

            //Act
            summaryAccount.Update(ormMock);

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not interact with the ORM.
            ormMock.DidNotReceive().Update(Arg.Any<SummaryAccount>());
        }

        [Fact]
        public void Update_Null_DoesNothing()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;

            //Act

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not throw an exception if the given ORM is not valid.
            summaryAccount.Update(null);
        }

        [Fact]
        public void Delete_Orm_DoesNotCallDeleteOnOrm()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;
            IBffOrm ormMock = BffOrmMoq.NakedFake;

            //Act
            summaryAccount.Delete(ormMock);

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not interact with the ORM.
            ormMock.DidNotReceive().Delete(Arg.Any<SummaryAccount>());
        }

        [Fact]
        public void Delete_Null_DoesNothing()
        {
            //Arrange
            SummaryAccount summaryAccount = SummaryAccountFactory;

            //Act

            //Assert
            //SummaryAccount is a virtual Account, which summarizes all over accounts. Thus, it should not throw an exception if the given ORM is not valid.
            summaryAccount.Delete(null);
        }
    }
}