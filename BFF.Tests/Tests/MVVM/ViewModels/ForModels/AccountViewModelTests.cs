using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Mocks.MVVM.ViewModels.ForModels;
using Moq;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class AccountViewModelTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> AccountViewModelData =
                AccountMoq.Mocks.Select(am => new object[] {am.Object});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ConstructionTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock.Object);

                //Act

                //Assert
                Assert.Equal(account.Id, accountViewModel.Id);
                Assert.Equal(account.Name, accountViewModel.Name);
                Assert.Equal(account.StartingBalance, accountViewModel.StartingBalance);
            }
        }

        public class ToStringTest
        {
            public static IEnumerable<object[]> AccountViewModelData =
                AccountMoq.Mocks.Select(am => new object[] {am.Object});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ToStringTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock.Object);

                //Act

                //Assert
                Assert.Equal(account.Name, accountViewModel.ToString());
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudInsertFact()
            {
                //Arrange
                Mock<ICommonPropertyProvider> commonPropertyProviderMock =
                    CommonPropertyProviderMoq.Mock;
                AccountViewModel insertAccount = new AccountViewModel(AccountMoq.NotInsertedAccountMock.Object,
                                                                      BffOrmMoq.CreateMock(commonPropertyProviderMock).Object);
                commonPropertyProviderMock.ResetCalls();

                //Act
                insertAccount.Insert();

                //Assert
                commonPropertyProviderMock.Verify(cpp => cpp.Add(It.IsAny<IAccount>()), Times.Once());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //Arrange
                Mock<IAccount> mock = AccountMoq.Mocks[0];
                AccountViewModel updateAccount = new AccountViewModel(mock.Object, BffOrmMoq.Mock.Object);
                mock.ResetCalls();

                //Act
                updateAccount.Name = "asdf";

                //Assert
                mock.Verify(orm => orm.Update(It.IsAny<IBffOrm>()), Times.Once);
            }
            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                Mock<IAccount> accountMock = AccountMoq.Mocks[0];
                Mock<ICommonPropertyProvider> commonPropertyProviderMock =
                    CommonPropertyProviderMoq.Mock;
                AccountViewModel deleteAccount = new AccountViewModel(accountMock.Object,
                                                                      BffOrmMoq.CreateMock(commonPropertyProviderMock).Object);
                commonPropertyProviderMock.ResetCalls();

                //Act
                deleteAccount.Delete();

                //Assert
                commonPropertyProviderMock.Verify(cpp => cpp.Remove(It.IsAny<IAccount>()), Times.Once());
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Mock<IAccount> accountMock = AccountMoq.Mocks[0];
                Mock<ICommonPropertyProvider> commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(summaryAccountViewModelMock:
                                                         SummaryAccountViewModelMoq.Mock);
                AccountViewModel accountViewModel = new AccountViewModel(accountMock.Object,
                                                                         BffOrmMoq.CreateMock(commonPropertyProviderMock).Object);

                //Act + Assert
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "AnotherAccountViewModel");
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 323);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                Mock<IAccount> mock = AccountMoq.Mocks[0];
                AccountViewModel accountViewModel = new AccountViewModel(mock.Object, BffOrmMoq.Mock.Object);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "Bank"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 32369));
            }
        }

        public class ValidToInsertTests
        {
            public static IEnumerable<object[]> ValidToInsert =
                AccountMoq.Mocks.Select(am => new object[] { am.Object });
            public static IEnumerable<object[]> NotValidToInsert =
                AccountMoq.NotValidToInsertMocks.Select(am => new object[] { am.Object });

            [Theory, MemberData(nameof(ValidToInsert))]
            public void ValidToInsertTest(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock.Object);

                //Act

                //Assert
                Assert.True(accountViewModel.ValidToInsert());
            }

            [Theory, MemberData(nameof(NotValidToInsert))]
            public void NotValidToInsertTest(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock.Object);

                //Act

                //Assert
                Assert.False(accountViewModel.ValidToInsert());
            }
        }

        public class RefreshTests
        {
            [Fact]
            public void RefreshBalanceTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool eventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if(args.PropertyName == nameof(AccountViewModel.Balance)) eventRaised = true;
                };

                //Act
                accountViewModel.RefreshBalance();

                //Assert
                Assert.True(eventRaised);
            }
            
            [Fact]
            public void RefreshTitsTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool eventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Tits)) eventRaised = true;
                };

                //Act
                accountViewModel.RefreshTits();

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshBalanceMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool eventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Balance)) eventRaised = true;
                };

                //Act
                Messenger.Default.Send(AccountMessage.RefreshBalance, accountViewModel.Account);

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshTitsMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool eventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Tits)) eventRaised = true;
                };

                //Act
                Messenger.Default.Send(AccountMessage.RefreshTits, accountViewModel.Account);

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(AccountViewModel.Balance)) balanceEventRaised = true;
                };

                //Act
                Messenger.Default.Send(AccountMessage.Refresh, accountViewModel.Account);

                //Assert
                Assert.True(titsEventRaised);
                Assert.True(balanceEventRaised);
            }

            [Fact]
            public void RefreshCultureMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                bool startingBalanceEventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(AccountViewModel.Balance)) balanceEventRaised = true;
                    if (args.PropertyName == nameof(AccountViewModel.StartingBalance)) startingBalanceEventRaised = true;
                };

                //Act
                Messenger.Default.Send(CutlureMessage.Refresh, null);

                //Assert
                Assert.True(titsEventRaised);
                Assert.True(balanceEventRaised);
                Assert.True(startingBalanceEventRaised);
            }

            [Fact]
            public void RefreshCurrencyMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                bool startingBalanceEventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(AccountViewModel.Balance)) balanceEventRaised = true;
                    if (args.PropertyName == nameof(AccountViewModel.StartingBalance)) startingBalanceEventRaised = true;
                };

                //Act
                Messenger.Default.Send(CutlureMessage.RefreshCurrency, null);

                //Assert
                Assert.True(titsEventRaised);
                Assert.True(balanceEventRaised);
                Assert.True(startingBalanceEventRaised);
            }

            [Fact]
            public void RefreshDateMessageTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);
                bool dateEventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AccountViewModel.IsDateFormatLong)) dateEventRaised = true;
                };

                //Act
                Messenger.Default.Send(CutlureMessage.RefreshDate, null);

                //Assert
                Assert.True(dateEventRaised);
            }
        }

        public class NewCommandTests
        {
            [Fact]
            public void NewTransactionCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);

                //Act
                accountViewModel.NewTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(TransactionViewModel), tit));
            }

            [Fact]
            public void NewIncomeCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);

                //Act
                accountViewModel.NewIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(IncomeViewModel), tit));
            }

            [Fact]
            public void NewTransferCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);

                //Act
                accountViewModel.NewTransferCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(TransferViewModel), tit));
            }

            [Fact]
            public void NewParentTransactionCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);

                //Act
                accountViewModel.NewParentTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(ParentTransactionViewModel), tit));
            }

            [Fact]
            public void NewParentIncomeCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.Mock.Object);

                //Act
                accountViewModel.NewParentIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(ParentIncomeViewModel), tit));
            }

            [Fact(Skip= "I have to figure out how to test this yet.")]
            public void ApplyCommandTest()
            {
                //Arrange
                Mock<ICommonPropertyProvider> commonPropertyProviderMock = CommonPropertyProviderMoq.CreateMock(
                    summaryAccountViewModelMock: SummaryAccountViewModelMoq.Mock);
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0].Object, BffOrmMoq.CreateMock(commonPropertyProviderMock).Object);
                accountViewModel.NewTransactionCommand.Execute(null);
                ITransactionViewModel transactionViewModel = (ITransactionViewModel) accountViewModel.NewTits[0];
                transactionViewModel.Category = CategoryViewModelMoq.Mocks[0].Object;
                transactionViewModel.Payee = PayeeViewModelMoq.Mocks[0].Object;

                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                accountViewModel.PropertyChanged += (sender, args) =>
                {
                    switch(args.PropertyName)
                    {
                        case nameof(AccountViewModel.Tits):
                            titsEventRaised = true;
                            break;
                        case nameof(AccountViewModel.Balance):
                            balanceEventRaised = true;
                            break;
                    }
                };

                //Act
                accountViewModel.ApplyCommand.Execute(null);

                //Assert
                Assert.True(titsEventRaised);
                Assert.True(balanceEventRaised);
            }
        }
    }
}