using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class AccountTests
    {

        public class ConstructionTests
        {
            public static IEnumerable<object[]> AccountData => new[]
            {
                new object[] { 1L, "Account", -6969L },
                new object[] { 2L, "Cash", 2303L },
                new object[] { -1L, "CreditCard", 5000L }
            };

            [Theory, MemberData(nameof(AccountData))]
            public void ConstructionTheory(long id, string name, long startingBalance)
            {  
                //Arrange
                Account account = new Account(id, name, startingBalance);
               
                //Act
               
                //Assert
                Assert.True(account.Id == id, $"{nameof(account.Id)} not set right!");
                Assert.True(account.Name == name, $"{nameof(account.Name)} not set right!");
                Assert.True(account.StartingBalance == startingBalance, $"{nameof(account.StartingBalance)} not set right!");
            }  

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Account account = new Account();

                //Act

                //Assert
                Assert.True(account.Id == -1L, $"Default {nameof(account.Id)} not set right!");
                Assert.True(account.Name == null, $"Default {nameof(account.Name)} not set right!");
                Assert.True(account.StartingBalance == 0L, $"Default {nameof(account.StartingBalance)} not set right!");
            }
        }

        public class ToStringTest
        {

            public static IEnumerable<object[]> AccountData => new[]
            {
                new object[] { 1L, "Account", -6969L },
                new object[] { 2L, "Cash", 2303L },
                new object[] { -1L, "CreditCard", 5000L }
            };

            [Theory, MemberData(nameof(AccountData))]
            public void ToStringTheory(long id, string name, long startingBalance)
            {
                //Arrange
                Account account = new Account(id, name, startingBalance);

                //Act

                //Assert
                Assert.True(account.ToString() == name, $"{nameof(account.ToString)}() does not work right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Account account = new Account(1, "AnAccount", 6969);
                Mock<IBffOrm> ormMock = OrmMock();

                //Act
                account.Insert(ormMock.Object);
                account.Update(ormMock.Object);
                account.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Account>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Account>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Account>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Account account = new Account(1, "AnAccount", 6969);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => account.Insert(null));
                Assert.Throws<ArgumentNullException>(() => account.Update(null));
                Assert.Throws<ArgumentNullException>(() => account.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Account account = new Account(1, "AnAccount", 6969);

                //Act + Assert
                Assert.PropertyChanged(account, nameof(account.Id), () => account.Id = 69);
                Assert.PropertyChanged(account, nameof(account.Name), () => account.Name = "AnotherAccount");
                Assert.PropertyChanged(account, nameof(account.StartingBalance), () => account.StartingBalance = 323);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                Account account = new Account(1, "AnAccount", 6969);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException), 
                    () => Assert.PropertyChanged(account, nameof(account.Id), () => account.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(account, nameof(account.Name), () => account.Name = "AnAccount"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(account, nameof(account.StartingBalance), () => account.StartingBalance = 6969));
            }
        }

        public static Mock<IBffOrm> OrmMock()
        {
            Mock<IBffOrm> ormMock = new Mock<IBffOrm>();
            ormMock.Setup(orm => orm.Insert(It.IsAny<Account>())).Verifiable();
            ormMock.Setup(orm => orm.Update(It.IsAny<Account>())).Verifiable();
            ormMock.Setup(orm => orm.Delete(It.IsAny<Account>())).Verifiable();
            
            return ormMock;
        }
    }
}
