using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class TransIncBaseTests<T> : TitBaseTests<T> where T : TransIncBase
    {
        protected abstract T TransIncBaseFactory { get; }

        protected abstract long AccountIdInitialValue { get; }
        protected abstract long AccountIdDifferentValue { get; }

        [Fact]
        public void AccountId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            transIncBase.AccountId = AccountIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncBase.AccountId = AccountIdDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncBase, nameof(transIncBase.AccountId), shouldTriggerNotification);
        }

        [Fact]
        public void AccountId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            transIncBase.AccountId = AccountIdInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncBase.AccountId = AccountIdInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncBase, nameof(transIncBase.AccountId), shouldNotTriggerNotification);
        }

        protected abstract long PayeeIdInitialValue { get; }
        protected abstract long PayeeIdDifferentValue { get; }

        [Fact]
        public void PayeeId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            transIncBase.PayeeId = PayeeIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncBase.PayeeId = PayeeIdDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncBase, nameof(transIncBase.PayeeId), shouldTriggerNotification);
        }

        [Fact]
        public void PayeeId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            transIncBase.PayeeId = PayeeIdInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncBase.PayeeId = PayeeIdInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncBase, nameof(transIncBase.PayeeId), shouldNotTriggerNotification);
        }
    }
}
