using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TransIncBaseViewModelTests<T> : TitBaseViewModelTests<T> where T : TransIncBaseViewModel
    {
        protected abstract (T, ITransIncBase) TransIncBaseViewModelFactory { get; }

        protected abstract IAccountViewModel AccountInitialValue { get; }
        protected abstract IAccountViewModel AccountDifferentValue { get; }

        [Fact]
        public void AccountGet_CallsModelAccountGet()
        {
            //Arrange
            (T viewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;

            //Act
            _ = viewModel.Account;

            //Assert
            _ = mock.Received().AccountId;
        }

        [Fact]
        public void AccountSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T transIncBaseViewModel, _) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Account = AccountInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncBaseViewModel.Account = AccountDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncBaseViewModel, nameof(transIncBaseViewModel.Account), shouldTriggerNotification);
        }

        [Fact]
        public void AccountSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T transIncBaseViewModel, _) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Account = AccountInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncBaseViewModel.Account = AccountInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncBaseViewModel, nameof(transIncBaseViewModel.Account), shouldNotTriggerNotification);
        }

        [Fact]
        public void AccountSet_ChangedValue_CallsModelAccount()
        {
            //Arrange
            (T transIncBaseViewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Account = AccountInitialValue;

            //Act
            transIncBaseViewModel.Account = AccountDifferentValue;

            //Assert
            mock.Received().AccountId = AccountDifferentValue.Id;
        }

        [Fact]
        public void AccountSet_SameValue_DoesNtCallModelAccount()
        {
            //Arrange
            (T transIncBaseViewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Account = AccountInitialValue;

            //Act
            transIncBaseViewModel.Account = AccountInitialValue;

            //Assert
            mock.DidNotReceive().AccountId = AccountInitialValue.Id;
        }

        protected abstract IPayeeViewModel PayeeInitialValue { get; }
        protected abstract IPayeeViewModel PayeeDifferentValue { get; }

        [Fact]
        public void PayeeGet_CallsModelPayeeGet()
        {
            //Arrange
            (T viewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;

            //Act
            _ = viewModel.Payee;

            //Assert
            _ = mock.Received().PayeeId;
        }

        [Fact]
        public void PayeeSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T transIncBaseViewModel, _) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Payee = PayeeInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncBaseViewModel.Payee = PayeeDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncBaseViewModel, nameof(transIncBaseViewModel.Payee), shouldTriggerNotification);
        }

        [Fact]
        public void PayeeSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T transIncBaseViewModel, _) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Payee = PayeeInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncBaseViewModel.Payee = PayeeInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncBaseViewModel, nameof(transIncBaseViewModel.Payee), shouldNotTriggerNotification);
        }

        [Fact]
        public void PayeeSet_ChangedValue_CallsModelPayee()
        {
            //Arrange
            (T transIncBaseViewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Payee = PayeeInitialValue;

            //Act
            transIncBaseViewModel.Payee = PayeeDifferentValue;

            //Assert
            mock.Received().PayeeId = PayeeDifferentValue.Id;
        }

        [Fact]
        public void PayeeSet_SameValue_DoesNtCallModelPayee()
        {
            //Arrange
            (T transIncBaseViewModel, ITransIncBase mock) = TransIncBaseViewModelFactory;
            transIncBaseViewModel.Payee = PayeeInitialValue;

            //Act
            transIncBaseViewModel.Payee = PayeeInitialValue;

            //Assert
            mock.DidNotReceive().PayeeId = PayeeInitialValue.Id;
        }
    }
}