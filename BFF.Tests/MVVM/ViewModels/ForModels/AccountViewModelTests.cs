using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.DB.Mock;
using BFF.Tests.MVVM.Models.Native.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.ViewModels.ForModels
{
    public class AccountViewModelTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> AccountViewModelData => AccountMoq.Accounts.Select(a => new object[] { a });

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ConstructionTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.BffOrm);

                //Act

                //Assert
                Assert.Equal(account.Id, accountViewModel.Id);
                Assert.Equal(account.Name, accountViewModel.Name);
                Assert.Equal(account.StartingBalance, accountViewModel.StartingBalance);
            }
        }

        public class ToStringTest
        {
            public static IEnumerable<object[]> AccountViewModelData => AccountMoq.Accounts.Select(a => new object[] { a });

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ToStringTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.BffOrm);

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
                AccountViewModel insertAccount = new AccountViewModel(AccountMoq.NotInsertedAccount, BffOrmMoq.BffOrm);
                CommonPropertyProviderMoq.CommonPropertyProviderMock.ResetCalls();

                //Act
                insertAccount.Insert();

                //Assert
                CommonPropertyProviderMoq.CommonPropertyProviderMock.Verify(cpp => cpp.Add(It.IsAny<IAccount>()), Times.Once());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //Arrange
                AccountViewModel updateAccount = new AccountViewModel(AccountMoq.Accounts[0], BffOrmMoq.BffOrm);
                AccountMoq.AccountMocks[0].ResetCalls();

                //Act
                updateAccount.Name = "asdf";

                //Assert
                AccountMoq.AccountMocks[0].Verify(orm => orm.Update(It.IsAny<IBffOrm>()), Times.Once);
            }
            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                AccountViewModel deleteAccount = new AccountViewModel(AccountMoq.Accounts[0], BffOrmMoq.BffOrm);
                CommonPropertyProviderMoq.CommonPropertyProviderMock.ResetCalls();

                //Act
                deleteAccount.Delete();

                //Assert
                CommonPropertyProviderMoq.CommonPropertyProviderMock.Verify(cpp => cpp.Remove(It.IsAny<IAccount>()), Times.Once());
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Accounts[0], BffOrmMoq.BffOrm);

                //Act + Assert
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "AnotherAccountViewModel");
                Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 323);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(AccountMoq.Accounts[0], BffOrmMoq.BffOrm);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "Bank"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 32369));
            }
        }
    }
}