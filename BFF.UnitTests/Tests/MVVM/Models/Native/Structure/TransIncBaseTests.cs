using BFF.MVVM.Models.Native.Structure;
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
            bool notified = false;
            transIncBase.AccountId = AccountIdInitialValue;
            transIncBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransIncBase.AccountId)) notified = true;
            };

            //Act
            transIncBase.AccountId = AccountIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void AccountId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            bool notified = false;
            transIncBase.AccountId = AccountIdInitialValue;
            transIncBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransIncBase.AccountId)) notified = true;
            };

            //Act
            transIncBase.AccountId = AccountIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        protected abstract long PayeeIdInitialValue { get; }
        protected abstract long PayeeIdDifferentValue { get; }

        [Fact]
        public void PayeeId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            bool notified = false;
            transIncBase.PayeeId = PayeeIdInitialValue;
            transIncBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransIncBase.PayeeId)) notified = true;
            };

            //Act
            transIncBase.PayeeId = PayeeIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void PayeeId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transIncBase = TransIncBaseFactory;
            bool notified = false;
            transIncBase.PayeeId = PayeeIdInitialValue;
            transIncBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransIncBase.PayeeId)) notified = true;
            };

            //Act
            transIncBase.PayeeId = PayeeIdInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}
