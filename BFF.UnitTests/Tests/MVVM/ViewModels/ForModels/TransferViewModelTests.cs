using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Helper;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded")]
    public class TransferViewModelTests : TitBaseViewModelTests<TransferViewModel>
    {
        protected override (TransferViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            ITransfer transferMock = Substitute.For<ITransfer>();
            transferMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new TransferViewModel(transferMock, bffOrm), transferMock, bffOrm);
        }

        protected override (TransferViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                ITransfer transferMock = Substitute.For<ITransfer>();
                transferMock.Memo.Returns(MemoInitialValue);
                return (new TransferViewModel(transferMock, Substitute.For<IBffOrm>()), transferMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (TransferViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                ITransfer transferMock = Substitute.For<ITransfer>();
                transferMock.Cleared.Returns(ClearedInitialValue);
                transferMock.Date.Returns(DateInitialValue);
                return (new TransferViewModel(transferMock, Substitute.For<IBffOrm>()), transferMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);

        private (TransferViewModel, ITransfer) TransferViewModelFactory
        {
            get
            {
                ITransfer transferMock = Substitute.For<ITransfer>();
                transferMock.ToAccountId.Returns(c => ToAccountInitialValue.Id);
                transferMock.FromAccountId.Returns(c => FromAccountInitialValue.Id);
                transferMock.Sum.Returns(c => SumInitialValue);
                return (new TransferViewModel(transferMock, Substitute.For<IBffOrm>()), transferMock);
            }
        }

        private IAccountViewModel ToAccountInitialValue
        {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(23);
                return accountViewModel;
            }
        }
        private IAccountViewModel ToAccountDifferentValue
        {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(69);
                return accountViewModel;
            }
        }

        [Fact]
        public void ToAccountGet_CallsModelToAccountGet()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;

            //Act
            _ = viewModel.ToAccount;

            //Assert
            _ = mock.Received().ToAccountId;
        }

        [Fact]
        public void ToAccountSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.ToAccount = ToAccountInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.ToAccount = ToAccountDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.ToAccount), shouldTriggerNotification);
        }

        [Fact]
        public void ToAccountSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.ToAccount = ToAccountInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.ToAccount = ToAccountInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.ToAccount), shouldNotTriggerNotification);
        }

        [Fact]
        public void ToAccountSet_ChangedValue_CallsModelToAccount()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.ToAccount = ToAccountInitialValue;

            //Act
            viewModel.ToAccount = ToAccountDifferentValue;

            //Assert
            mock.Received().ToAccountId = ToAccountDifferentValue.Id;
        }

        [Fact]
        public void ToAccountSet_SameValue_DoesNtCallModelToAccount()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.ToAccount = ToAccountInitialValue;

            //Act
            viewModel.ToAccount = ToAccountInitialValue;

            //Assert
            mock.DidNotReceive().ToAccountId = ToAccountInitialValue.Id;
        }

        private IAccountViewModel FromAccountInitialValue
        {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(3);
                return accountViewModel;
            }
        }
        private IAccountViewModel FromAccountDifferentValue
        {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(6);
                return accountViewModel;
            }
        }

        [Fact]
        public void FromAccountGet_CallsModelFromAccountGet()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;

            //Act
            _ = viewModel.FromAccount;

            //Assert
            _ = mock.Received().FromAccountId;
        }

        [Fact]
        public void FromAccountSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.FromAccount = FromAccountInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.FromAccount = FromAccountDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.FromAccount), shouldTriggerNotification);
        }

        [Fact]
        public void FromAccountSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.FromAccount = FromAccountInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.FromAccount = FromAccountInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.FromAccount), shouldNotTriggerNotification);
        }

        [Fact]
        public void FromAccountSet_ChangedValue_CallsModelFromAccount()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.FromAccount = FromAccountInitialValue;

            //Act
            viewModel.FromAccount = FromAccountDifferentValue;

            //Assert
            mock.Received().FromAccountId = FromAccountDifferentValue.Id;
        }

        [Fact]
        public void FromAccountSet_SameValue_DoesNtCallModelFromAccount()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.FromAccount = FromAccountInitialValue;

            //Act
            viewModel.FromAccount = FromAccountInitialValue;

            //Assert
            mock.DidNotReceive().FromAccountId = FromAccountInitialValue.Id;
        }

        [Fact]
        public void ToAccountSet_FromAccount_SwitchesTheTwoAccounts()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;

            //Act
            viewModel.ToAccount = FromAccountInitialValue;

            //Assert
            mock.Received().FromAccountId = ToAccountInitialValue.Id;
        }

        [Fact]
        public void FromAccountSet_ToAccount_SwitchesTheTwoAccounts()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;

            //Act
            viewModel.FromAccount = ToAccountInitialValue;

            //Assert
            mock.Received().ToAccountId = FromAccountInitialValue.Id;
        }

        private long SumInitialValue => 23;

        private long SumDifferentValue => 69;

        [Fact]
        public void SumGet_CallsModelSumGet()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;

            //Act
            _ = viewModel.Sum;

            //Assert
            _ = mock.Received().Sum;
        }

        [Fact]
        public void SumSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.Sum = SumDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.Sum), shouldTriggerNotification);
        }

        [Fact]
        public void SumSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (TransferViewModel viewModel, _) = TransferViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.Sum = SumInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.Sum), shouldNotTriggerNotification);
        }

        [Fact]
        public void SumSet_ChangedValue_CallsModelSum()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            viewModel.Sum = SumDifferentValue;

            //Assert
            mock.Received().Sum = SumDifferentValue;
        }

        [Fact]
        public void SumSet_SameValue_DoesNtCallModelSum()
        {
            //Arrange
            (TransferViewModel viewModel, ITransfer mock) = TransferViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            viewModel.Sum = SumInitialValue;

            //Assert
            mock.DidNotReceive().Sum = SumInitialValue;
        }

        public new static IEnumerable<object[]> AtLeastOneNullCommonPropertyProvider
            => new[]
            {
                new object [] {CommonPropertyProviderMoq.NullAccountViewModel}
            };

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public override void Insert_AtLeastOneNullCommonPropertyProvider_NotInserted_DoesntCallInsertOnOrm(ICommonPropertyProvider commonPropertyProvider)
        {
            base.Insert_AtLeastOneNullCommonPropertyProvider_NotInserted_DoesntCallInsertOnOrm(commonPropertyProvider);
        }

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public override void ValidToInsert_AtLeastOneNullCommonPropertyProvider_NotInserted_False(ICommonPropertyProvider commonPropertyProvider)
        {
            base.ValidToInsert_AtLeastOneNullCommonPropertyProvider_NotInserted_False(commonPropertyProvider);
        }

    }
}
