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
    public static class PayeeViewModelTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> PayeeViewModelData =
                PayeeMoq.Mocks.Select(am => new object[] {am});

            [Theory, MemberData(nameof(PayeeViewModelData))]
            public void ConstructionTheory(IPayee payee)
            {
                //Arrange
                PayeeViewModel payeeViewModel = new PayeeViewModel(payee, BffOrmMoq.Mock);

                //Act

                //Assert
                Assert.Equal(payee.Id, payeeViewModel.Id);
                Assert.Equal(payee.Name, payeeViewModel.Name);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudInsertFact()
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                PayeeViewModel insertPayee = new PayeeViewModel(PayeeMoq.NotInsertedAccountMock,
                                                                  BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                insertPayee.Insert();

                //Assert
                commonPropertyProviderMock.Received().Add(Arg.Any<IPayee>());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //Arrange
                IPayee mock = PayeeMoq.Mocks[0];
                PayeeViewModel updatePayee = new PayeeViewModel(mock, BffOrmMoq.Mock);

                //Act
                updatePayee.Name = "asdf";

                //Assert
                mock.Received().Update(Arg.Any<IBffOrm>());
            }
            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                IPayee payeeMock = PayeeMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                PayeeViewModel deletePayee = new PayeeViewModel(payeeMock, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                deletePayee.Delete();

                //Assert
                commonPropertyProviderMock.Received().Remove(Arg.Any<IPayee>());
            }
        }

        public class PropertyTests
        {

            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                IPayee payeeMock = PayeeMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(summaryAccountViewModelMock:
                                                         SummaryAccountViewModelMoq.Mock);
                PayeeViewModel payeeViewModel = new PayeeViewModel(payeeMock,
                                                                         BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act + Assert
                Assert.PropertyChanged(payeeViewModel, nameof(payeeViewModel.Name), () => payeeViewModel.Name = "AnotherPayeeViewModel");
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                IPayee mock = PayeeMoq.Mocks[0];
                PayeeViewModel payeeViewModel = new PayeeViewModel(mock, BffOrmMoq.Mock);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(payeeViewModel, nameof(payeeViewModel.Name), () => payeeViewModel.Name = mock.Name));
            }
        }

        public class ValidToInsertTests
        {
            public static IEnumerable<object[]> ValidToInsert =
                PayeeMoq.Mocks.Select(am => new object[] { am });
            public static IEnumerable<object[]> NotValidToInsert =
                PayeeMoq.NotValidToInsertMocks.Select(am => new object[] { am });

            [Theory, MemberData(nameof(ValidToInsert))]
            public void ValidToInsertTest(IPayee payee)
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(payeeViewModelMocks: new List<IPayeeViewModel>());
                PayeeViewModel payeeViewModel = new PayeeViewModel(payee, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act

                //Assert
                Assert.True(payeeViewModel.ValidToInsert());
            }

            [Theory, MemberData(nameof(NotValidToInsert))]
            public void NotValidToInsertTest(IPayee payee)
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(payeeViewModelMocks: new List<IPayeeViewModel>());
                PayeeViewModel payeeViewModel = new PayeeViewModel(payee, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act

                //Assert
                Assert.False(payeeViewModel.ValidToInsert());
            }
        }
    }
}