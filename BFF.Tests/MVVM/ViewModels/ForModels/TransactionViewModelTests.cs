using System;
using System.Collections.Generic;
using System.Diagnostics;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using Moq;
using NLog;
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
                //Assemble
                Mock<IBffOrm> ormMock = MockTheOrm();
                Transaction transaction = new Transaction(-1, accountId, dateTime, payeeId, categoryId, memo, sum, cleared);
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

        private static Mock<IBffOrm> MockTheOrm()
        {
            Account account1 = new Account(1, "One", 1), 
                    account2 = new Account(2, "Two", 10),
                    account3 = new Account(3, "Three", 11);
            Payee payee1 = new Payee(1, "One"), 
                  payee2 = new Payee(2, "Two"),
                  payee3 = new Payee(3, "Three");
            Category category1 = new Category(1, -1, "One"), 
                     category2 = new Category(2, -1, "Two"),
                     category3 = new Category(3, 1, "Three");

            Mock<IBffOrm> ormMock = new Mock<IBffOrm>();

            ormMock.Setup(orm => orm.CommonPropertyProvider.GetAccount(1)).Returns(account1);
            ormMock.Setup(orm => orm.CommonPropertyProvider.GetAccount(2)).Returns(account2);
            ormMock.Setup(orm => orm.CommonPropertyProvider.GetAccount(3)).Returns(account3);

            ormMock.Setup(orm => orm.CommonPropertyProvider.GetPayee(1)).Returns(payee1);
            ormMock.Setup(orm => orm.CommonPropertyProvider.GetPayee(2)).Returns(payee2);
            ormMock.Setup(orm => orm.CommonPropertyProvider.GetPayee(3)).Returns(payee3);

            ormMock.Setup(orm => orm.GetCategory(1)).Returns(category1);
            ormMock.Setup(orm => orm.GetCategory(2)).Returns(category2);
            ormMock.Setup(orm => orm.GetCategory(3)).Returns(category3);

            return ormMock;
        }
    }
}
