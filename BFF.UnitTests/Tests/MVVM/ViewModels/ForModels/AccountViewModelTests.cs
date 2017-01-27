using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Mocks.MVVM.ViewModels.ForModels;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public static class AccountViewModelTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> AccountViewModelData =
                AccountMoq.Mocks.Select(am => new object[] {am});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ConstructionTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock);

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
                AccountMoq.Mocks.Select(am => new object[] {am});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ToStringTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock);

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
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                AccountViewModel insertAccount = new AccountViewModel(AccountMoq.NotInserted,
                                                                      BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                insertAccount.Insert();

                //Assert
                commonPropertyProviderMock.Received().Add(Arg.Any<IAccount>());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //Arrange
                IAccount mock = AccountMoq.Mocks[0];
                AccountViewModel updateAccount = new AccountViewModel(mock, BffOrmMoq.Mock);

                //Act
                updateAccount.Name = "asdf";

                //Assert
                mock.Received().Update(Arg.Any<IBffOrm>());
            }
            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                IAccount accountMock = AccountMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                AccountViewModel deleteAccount = new AccountViewModel(accountMock,
                                                                      BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                deleteAccount.Delete();

                //Assert
                commonPropertyProviderMock.Received().Remove(Arg.Any<IAccount>());
            }
        }

        public class PropertyTests
        {
            [Fact]
            public void BalancePropertyFact()
            {
                //Arrange
                IList<IAccount> accountMocks = AccountMoq.Mocks;
                IBffOrm ormMock = BffOrmMoq.CreateMock(accountMocks: accountMocks);
                AccountViewModel accountViewModel = new AccountViewModel(accountMocks[0],
                                                                         ormMock);

                //Act
                long? balance = accountViewModel.Balance;

                //Assert
                Assert.Equal(ormMock.GetAccountBalance(accountMocks[0]), balance);
            }

            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                IAccount accountMock = AccountMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(summaryAccountViewModelMock:
                                                         SummaryAccountViewModelMoq.Mock);
                AccountViewModel accountViewModel = new AccountViewModel(accountMock,
                                                                         BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act + Assert
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "AnotherAccountViewModel");
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 323);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                IAccount mock = AccountMoq.Mocks[0];
                AccountViewModel accountViewModel = new AccountViewModel(mock, BffOrmMoq.Mock);

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
                AccountMoq.Mocks.Select(am => new object[] { am });
            public static IEnumerable<object[]> NotValidToInsert =
                AccountMoq.NotValidToInsert.Select(am => new object[] { am });

            [Theory, MemberData(nameof(ValidToInsert))]
            public void ValidToInsertTest(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock);

                //Act

                //Assert
                Assert.True(accountViewModel.ValidToInsert());
            }

            [Theory, MemberData(nameof(NotValidToInsert))]
            public void NotValidToInsertTest(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.Mock);

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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);
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
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);

                //Act
                accountViewModel.NewTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(TransactionViewModel), tit));
            }

            [Fact]
            public void NewIncomeCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);

                //Act
                accountViewModel.NewIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(IncomeViewModel), tit));
            }

            [Fact]
            public void NewTransferCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);

                //Act
                accountViewModel.NewTransferCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(TransferViewModel), tit));
            }

            [Fact]
            public void NewParentTransactionCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);

                //Act
                accountViewModel.NewParentTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(ParentTransactionViewModel), tit));
            }

            [Fact]
            public void NewParentIncomeCommandTest()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Mocks[0], BffOrmMoq.Mock);

                //Act
                accountViewModel.NewParentIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(accountViewModel.NewTits, tit => Assert.IsType(typeof(ParentIncomeViewModel), tit));
            }
        }
    }
}