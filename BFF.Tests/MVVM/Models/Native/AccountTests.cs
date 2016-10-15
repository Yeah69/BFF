﻿using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
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
                Assert.Equal(id, account.Id);
                Assert.Equal(name, account.Name);
                Assert.Equal(startingBalance, account.StartingBalance);
            }  

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Account account = new Account();

                //Act

                //Assert
                Assert.Equal(-1L, account.Id);
                Assert.Equal(null, account.Name);
                Assert.Equal(0L, account.StartingBalance);
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
                Assert.Equal(name, account.ToString());
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Account account = new Account(1, "AnAccount", 6969);
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

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
    }
}
