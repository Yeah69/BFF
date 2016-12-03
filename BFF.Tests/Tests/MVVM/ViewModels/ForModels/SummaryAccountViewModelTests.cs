using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public static class SummaryAccountViewModelTests
    {
        public class ConstructionTests
        {

            [Fact]
            public void ConstructionFact()
            {
                //Arrange
                IList<IAccount> accountMocks = AccountMoq.Mocks;
                ICommonPropertyProvider commonPropertyProvider = CommonPropertyProviderMoq.CreateMock(accountMocks: accountMocks);
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.CreateMock(commonPropertyProvider));

                //Act

                //Assert
                Assert.Equal(-1, summaryAccountViewModel.Id);
                Assert.Equal("All Accounts", summaryAccountViewModel.Name);
                Assert.Equal(accountMocks.Sum(a => a.StartingBalance), summaryAccountViewModel.StartingBalance);
            }
        }

        public class ToStringTest
        {

            [Fact]
            public void ToStringTheory()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.CreateMock());

                //Act

                //Assert
                Assert.Equal("All Accounts", summaryAccountViewModel.ToString());
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudInsertFact()
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.CreateMock());

                //Act
                summaryAccountViewModel.Insert();

                //Assert
                commonPropertyProviderMock.DidNotReceive().Add(Arg.Any<IAccount>());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //SummaryAccount is virtual nothing should be updateable.
            }

            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                summaryAccountViewModel.Delete();

                //Assert
                commonPropertyProviderMock.DidNotReceive().Remove(Arg.Any<IAccount>());
            }
        }

        public class PropertyTests
        {
            [Fact]
            public void BalancePropertyFact()
            {
                //Arrange
                IList<IAccount> accountMocks = AccountMoq.Mocks;
                IList<ITransaction> transactionMocks = TransactionMoq.Mocks;
                IList<IIncome> incomeMocks = IncomeMoq.Mocks;
                IList<ITransfer> transferMocks = TransferMoq.Mocks;
                IList<IParentTransaction> parentTransactionMocks = ParentTransactionMoq.Mocks;
                IList<IParentIncome> parentIncomeMocks = ParentIncomeMoq.Mocks;
                IList<ISubTransaction> subTransactionMocks = SubTransactionMoq.Mocks;
                IList<ISubIncome> subIncomeMocks = SubIncomeMoq.Mocks;
                IBffOrm ormMock = BffOrmMoq.CreateMock(accountMocks: accountMocks, transactionMocks: transactionMocks,
                                                 incomeMocks: incomeMocks, transferMocks: transferMocks,
                                                 parentTransactionMocks: parentTransactionMocks,
                                                 parentIncomeMocks: parentIncomeMocks,
                                                 subTransactionMocks: subTransactionMocks, subIncomeMocks: subIncomeMocks);
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(ormMock);

                //Act
                long? balance = summaryAccountViewModel.Balance;

                //Assert
                Assert.Equal(ormMock.GetSummaryAccountBalance(), balance);
            }
        }

        public class ValidToInsertTests
        {

            [Fact]
            public void NeverValidToInsert()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act

                //Assert
                Assert.False(summaryAccountViewModel.ValidToInsert());
            }
        }

        public class RefreshTests
        {
            [Fact]
            public void RefreshBalanceTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool eventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if(args.PropertyName == nameof(SummaryAccountViewModel.Balance)) eventRaised = true;
                };

                //Act
                summaryAccountViewModel.RefreshBalance();

                //Assert
                Assert.True(eventRaised);
            }
            
            [Fact]
            public void RefreshTitsTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool eventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Tits)) eventRaised = true;
                };

                //Act
                summaryAccountViewModel.RefreshTits();

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshBalanceMessageTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool eventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Balance)) eventRaised = true;
                };

                //Act
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshTitsMessageTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool eventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Tits)) eventRaised = true;
                };

                //Act
                Messenger.Default.Send(SummaryAccountMessage.RefreshTits);

                //Assert
                Assert.True(eventRaised);
            }

            [Fact]
            public void RefreshMessageTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Balance)) balanceEventRaised = true;
                };

                //Act
                Messenger.Default.Send(SummaryAccountMessage.Refresh);

                //Assert
                Assert.True(titsEventRaised);
                Assert.True(balanceEventRaised);
            }

            [Fact]
            public void RefreshCultureMessageTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                bool startingBalanceEventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Balance)) balanceEventRaised = true;
                    if (args.PropertyName == nameof(SummaryAccountViewModel.StartingBalance)) startingBalanceEventRaised = true;
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
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool titsEventRaised = false;
                bool balanceEventRaised = false;
                bool startingBalanceEventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Tits)) titsEventRaised = true;
                    if (args.PropertyName == nameof(SummaryAccountViewModel.Balance)) balanceEventRaised = true;
                    if (args.PropertyName == nameof(SummaryAccountViewModel.StartingBalance)) startingBalanceEventRaised = true;
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
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);
                bool dateEventRaised = false;
                summaryAccountViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SummaryAccountViewModel.IsDateFormatLong)) dateEventRaised = true;
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
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act
                summaryAccountViewModel.NewTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(summaryAccountViewModel.NewTits, tit => Assert.IsType(typeof(TransactionViewModel), tit));
            }

            [Fact]
            public void NewIncomeCommandTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act
                summaryAccountViewModel.NewIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(summaryAccountViewModel.NewTits, tit => Assert.IsType(typeof(IncomeViewModel), tit));
            }

            [Fact]
            public void NewTransferCommandTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act
                summaryAccountViewModel.NewTransferCommand.Execute(null);

                //Assert
                Assert.Collection(summaryAccountViewModel.NewTits, tit => Assert.IsType(typeof(TransferViewModel), tit));
            }

            [Fact]
            public void NewParentTransactionCommandTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act
                summaryAccountViewModel.NewParentTransactionCommand.Execute(null);

                //Assert
                Assert.Collection(summaryAccountViewModel.NewTits, tit => Assert.IsType(typeof(ParentTransactionViewModel), tit));
            }

            [Fact]
            public void NewParentIncomeCommandTest()
            {
                //Arrange
                SummaryAccountViewModel summaryAccountViewModel = new SummaryAccountViewModel(BffOrmMoq.Mock);

                //Act
                summaryAccountViewModel.NewParentIncomeCommand.Execute(null);

                //Assert
                Assert.Collection(summaryAccountViewModel.NewTits, tit => Assert.IsType(typeof(ParentIncomeViewModel), tit));
            }
        }
    }
}