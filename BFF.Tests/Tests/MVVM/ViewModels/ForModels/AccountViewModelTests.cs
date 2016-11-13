using System.Collections.Generic;
using System.Linq;
using BFF.DB;
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
                AccountMoq.AccountMocks.Select(am => new object[] {am.Object});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ConstructionTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.BffOrmMock.Object);

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
                AccountMoq.AccountMocks.Select(am => new object[] {am.Object});

            [Theory, MemberData(nameof(AccountViewModelData))]
            public void ToStringTheory(IAccount account)
            {
                //Arrange
                AccountViewModel accountViewModel = new AccountViewModel(account, BffOrmMoq.BffOrmMock.Object);

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
                    CommonPropertyProviderMoq.CommonPropertyProviderMock;
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
                Mock<IAccount> mock = AccountMoq.AccountMocks[0];
                AccountViewModel updateAccount = new AccountViewModel(mock.Object, BffOrmMoq.BffOrmMock.Object);
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
                Mock<IAccount> accountMock = AccountMoq.AccountMocks[0];
                Mock<ICommonPropertyProvider> commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CommonPropertyProviderMock;
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
                Mock<IAccount> accountMock = AccountMoq.AccountMocks[0];
                Mock<ICommonPropertyProvider> commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(summaryAccountViewModelMock:
                                                         SummaryAccountViewModelMoq.SummaryAccountViewModelMock);
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
                Mock<IAccount> mock = AccountMoq.AccountMocks[0];
                AccountViewModel accountViewModel = new AccountViewModel(mock.Object, BffOrmMoq.BffOrmMock.Object);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.Name), () => accountViewModel.Name = "Bank"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(accountViewModel, nameof(accountViewModel.StartingBalance), () => accountViewModel.StartingBalance = 32369));
            }
        }
    }
}