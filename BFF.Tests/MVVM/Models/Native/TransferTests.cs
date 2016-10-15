using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class TransferTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> TransferData => new[]
            {
                new object[] { 1, 1, 2, DateTime.Today, "Yeah, Party!", 6969L, true },
                new object[] { -1, -1, 23, DateTime.Today - TimeSpan.FromDays(69), "Long ago", 323L, false },
                new object[] { 11, 455, -1, DateTime.Today + TimeSpan.FromDays(69), "Great Transfer", 87978L, true }
            };

            [Theory, MemberData(nameof(TransferData))]
            public void ConstructionTheory(long id, long fromAccountId, long toAccountId, DateTime date, string memo, long sum, bool cleared)
            {
                //Arrange
                Transfer transfer = new Transfer(id, fromAccountId, toAccountId, date, memo, sum, cleared);

                //Act

                //Assert
                Assert.True(transfer.Id == id, $"{nameof(transfer.Id)} not set right!");
                Assert.True(transfer.FromAccountId == fromAccountId, $"{nameof(transfer.FromAccountId)} not set right!");
                Assert.True(transfer.ToAccountId == toAccountId, $"{nameof(transfer.ToAccountId)} not set right!");
                Assert.True(transfer.Date == date, $"{nameof(transfer.Date)} not set right!");
                Assert.True(transfer.Memo == memo, $"{nameof(transfer.Memo)} not set right!");
                Assert.True(transfer.Sum == sum, $"{nameof(transfer.Sum)} not set right!");
                Assert.True(transfer.Cleared == cleared, $"{nameof(transfer.Cleared)} not set right!");
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Transfer transfer = new Transfer(today);

                //Act

                //Assert
                Assert.True(transfer.Id == -1L, $"Default {nameof(transfer.Id)} not set right!");
                Assert.True(transfer.FromAccountId == -1L, $"Default {nameof(transfer.FromAccountId)} not set right!");
                Assert.True(transfer.ToAccountId == -1L, $"Default {nameof(transfer.ToAccountId)} not set right!");
                Assert.True(transfer.Date == today, $"Default {nameof(transfer.Date)} not set right!");
                Assert.True(transfer.Memo == null, $"Default {nameof(transfer.Memo)} not set right!");
                Assert.True(transfer.Sum == 0L, $"Default {nameof(transfer.Sum)} not set right!");
                Assert.True(transfer.Cleared == false, $"Default {nameof(transfer.Cleared)} not set right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Transfer transfer = new Transfer(1, 1, 2, DateTime.Today, "Yeah, Party!", 6969L, true);
                Mock<IBffOrm> ormMock = IBffOrmMock.BffOrmMock;

                //Act
                transfer.Insert(ormMock.Object);
                transfer.Update(ormMock.Object);
                transfer.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Transfer>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Transfer>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Transfer>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Transfer transfer = new Transfer(1, 1, 2, DateTime.Today, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => transfer.Insert(null));
                Assert.Throws<ArgumentNullException>(() => transfer.Update(null));
                Assert.Throws<ArgumentNullException>(() => transfer.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Transfer transfer = new Transfer(1, 1, 2, DateTime.Today, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.PropertyChanged(transfer, nameof(transfer.Id), () => transfer.Id = 69);
                Assert.PropertyChanged(transfer, nameof(transfer.FromAccountId), () => transfer.FromAccountId = 69);
                Assert.PropertyChanged(transfer, nameof(transfer.ToAccountId), () => transfer.ToAccountId = 23);
                Assert.PropertyChanged(transfer, nameof(transfer.Date), () => transfer.Date = DateTime.Today - TimeSpan.FromDays(3));
                Assert.PropertyChanged(transfer, nameof(transfer.Memo), () => transfer.Memo = "Hangover?");
                Assert.PropertyChanged(transfer, nameof(transfer.Sum), () => transfer.Sum = 69L);
                Assert.PropertyChanged(transfer, nameof(transfer.Cleared), () => transfer.Cleared = false);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Transfer transfer = new Transfer(1, 1, 2, DateTime.Today, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.Id), () => transfer.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.FromAccountId), () => transfer.FromAccountId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.ToAccountId), () => transfer.ToAccountId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.Date), () => transfer.Date = today));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.Memo), () => transfer.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.Sum), () => transfer.Id = 6969L));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(transfer, nameof(transfer.Cleared), () => transfer.Cleared = true));
            }
        }
    }
}