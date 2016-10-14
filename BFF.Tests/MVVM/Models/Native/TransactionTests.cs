using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class TransactionTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> TransactionData => new[]
            {
                new object[] { 1, 1, DateTime.Today, 2, 3, "Yeah, Party!", -6969L, true },
                new object[] { -1, -1, DateTime.Today - TimeSpan.FromDays(69), 23, -1, "Long ago", -323L, false },
                new object[] { 11, 455, DateTime.Today + TimeSpan.FromDays(69), -1, 3, "Positive Transaction", 87978L, true }
            };

            [Theory, MemberData(nameof(TransactionData))]
            public void ConstructionTheory(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
            {
                //Arrange
                Transaction transaction = new Transaction(id, accountId, date, payeeId, categoryId, memo, sum, cleared);

                //Act

                //Assert
                Assert.True(transaction.Id == id, $"{nameof(transaction.Id)} not set right!");
                Assert.True(transaction.AccountId == accountId, $"{nameof(transaction.AccountId)} not set right!");
                Assert.True(transaction.Date == date, $"{nameof(transaction.Date)} not set right!");
                Assert.True(transaction.PayeeId == payeeId, $"{nameof(transaction.PayeeId)} not set right!");
                Assert.True(transaction.CategoryId == categoryId, $"{nameof(transaction.CategoryId)} not set right!");
                Assert.True(transaction.Memo == memo, $"{nameof(transaction.Memo)} not set right!");
                Assert.True(transaction.Sum == sum, $"{nameof(transaction.Sum)} not set right!");
                Assert.True(transaction.Cleared == cleared, $"{nameof(transaction.Cleared)} not set right!");
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Transaction transaction = new Transaction(today);

                //Act

                //Assert
                Assert.True(transaction.Id == -1L, $"Default {nameof(transaction.Id)} not set right!");
                Assert.True(transaction.AccountId == -1L, $"Default {nameof(transaction.AccountId)} not set right!");
                Assert.True(transaction.Date == today, $"Default {nameof(transaction.Date)} not set right!");
                Assert.True(transaction.PayeeId == -1L, $"Default {nameof(transaction.PayeeId)} not set right!");
                Assert.True(transaction.CategoryId == -1L, $"Default {nameof(transaction.CategoryId)} not set right!");
                Assert.True(transaction.Memo == null, $"Default {nameof(transaction.Memo)} not set right!");
                Assert.True(transaction.Sum == 0L, $"Default {nameof(transaction.Sum)} not set right!");
                Assert.True(transaction.Cleared == false, $"Default {nameof(transaction.Cleared)} not set right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Transaction transaction = new Transaction(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);
                Mock<IBffOrm> ormMock = IBffOrmMock.BffOrmMock;

                //Act
                transaction.Insert(ormMock.Object);
                transaction.Update(ormMock.Object);
                transaction.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Transaction>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Transaction>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Transaction>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Transaction transaction = new Transaction(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => transaction.Insert(null));
                Assert.Throws<ArgumentNullException>(() => transaction.Update(null));
                Assert.Throws<ArgumentNullException>(() => transaction.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Transaction transaction = new Transaction(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.PropertyChanged(transaction, nameof(transaction.Id), () => transaction.Id = 69);
                Assert.PropertyChanged(transaction, nameof(transaction.AccountId), () => transaction.AccountId = 69);
                Assert.PropertyChanged(transaction, nameof(transaction.Date), () => transaction.Date = DateTime.Today - TimeSpan.FromDays(3));
                Assert.PropertyChanged(transaction, nameof(transaction.PayeeId), () => transaction.PayeeId = 69);
                Assert.PropertyChanged(transaction, nameof(transaction.CategoryId), () => transaction.CategoryId = 69);
                Assert.PropertyChanged(transaction, nameof(transaction.Memo), () => transaction.Memo = "Hangover?");
                Assert.PropertyChanged(transaction, nameof(transaction.Sum), () => transaction.Sum = 69L);
                Assert.PropertyChanged(transaction, nameof(transaction.Cleared), () => transaction.Cleared = false);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Transaction transaction = new Transaction(1, 1, today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Id), () => transaction.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.AccountId), () => transaction.AccountId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Date), () => transaction.Date = today));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.PayeeId), () => transaction.PayeeId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.CategoryId), () => transaction.CategoryId = 3));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Memo), () => transaction.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Sum), () => transaction.Id = 6969L));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Cleared), () => transaction.Cleared = true));
            }
        }
    }
}