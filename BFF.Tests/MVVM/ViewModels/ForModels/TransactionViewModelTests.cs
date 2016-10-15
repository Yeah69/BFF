using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.ViewModels.ForModels
{
    public class TransactionViewModelTests
    {
        public class PropertyTests
        {
            [Theory, MemberData(nameof(TransactionData))]
            public void PropertyTheory(long accountId, DateTime dateTime, long payeeId, long categoryId, string memo, long sum, bool cleared)
            {
                //Arrange
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;
                ITransaction transaction = new Transaction(-1, accountId, dateTime, payeeId, categoryId, memo, sum, cleared);
                TransactionViewModel transactionViewModel = new TransactionViewModel(transaction, ormMock.Object);

                //Act

                //Assert
                Assert.True(transactionViewModel.Account.Id == accountId);
                Assert.True(transactionViewModel.Date == dateTime);
                Assert.True(transactionViewModel.Payee.Id == payeeId);
                Assert.True(transactionViewModel.Category.Id == categoryId);
                Assert.True(transactionViewModel.Memo == memo);
                Assert.True(transactionViewModel.Sum == sum);
                Assert.True(transactionViewModel.Cleared == cleared);
            }

            public static IEnumerable<object[]> TransactionData => new[]
            {
                new object[] { 1, new DateTime(2016, 08, 14), 1, 1, "Marcella is crazy", 1024, true },
                new object[] { 2, DateTime.MaxValue, 3, 1, "Addison-Wesley", 518, false },
                new object[] { 3, DateTime.Now, 1, 2, "Yeah", 323, true },
                new object[] { 3, DateTime.MinValue, 2, 1, "Refactoring by Martin Fowler", 69, false }
            };
        }
    }
}
