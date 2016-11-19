using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using Moq;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
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
                Assert.Equal(id, transaction.Id);
                Assert.Equal(accountId, transaction.AccountId);
                Assert.Equal(date, transaction.Date);
                Assert.Equal(payeeId, transaction.PayeeId);
                Assert.Equal(categoryId, transaction.CategoryId);
                Assert.Equal(memo, transaction.Memo);
                Assert.Equal(sum, transaction.Sum);
                Assert.Equal(cleared, transaction.Cleared);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Transaction transaction = new Transaction(today);

                //Act

                //Assert
                Assert.Equal(-1L, transaction.Id);
                Assert.Equal(-1L, transaction.AccountId);
                Assert.Equal(today, transaction.Date);
                Assert.Equal(-1L, transaction.PayeeId);
                Assert.Equal(-1L, transaction.CategoryId);
                Assert.Equal(null, transaction.Memo);
                Assert.Equal(0L, transaction.Sum);
                Assert.Equal(false, transaction.Cleared);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Transaction transaction = new Transaction(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                transaction.Insert(ormMock.Object);
                transaction.Update(ormMock.Object);
                transaction.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Transaction>()), Times.Once);
                ormMock.Verify(orm => orm.Update(It.IsAny<Transaction>()), Times.Once);
                ormMock.Verify(orm => orm.Delete(It.IsAny<Transaction>()), Times.Once);
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
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Sum), () => transaction.Sum = 6969L));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transaction, nameof(transaction.Cleared), () => transaction.Cleared = true));
            }
        }
    }
}