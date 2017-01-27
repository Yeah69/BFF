using BFF.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class TransIncTests<T> : TransIncBaseTests<T> where T : TransInc
    {
        protected abstract T TransIncFactory { get; }

        protected abstract long CategoryIdInitialValue { get; }
        protected abstract long CategoryIdDifferentValue { get; }

        [Fact]
        public void CategoryId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            bool notified = false;
            transInc.CategoryId = CategoryIdInitialValue;
            transInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransInc.CategoryId)) notified = true;
            };

            //Act
            transInc.CategoryId = CategoryIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void CategoryId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            bool notified = false;
            transInc.CategoryId = CategoryIdInitialValue;
            transInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransInc.CategoryId)) notified = true;
            };

            //Act
            transInc.CategoryId = CategoryIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void Sum_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            bool notified = false;
            transInc.Sum = SumInitialValue;
            transInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransInc.Sum)) notified = true;
            };

            //Act
            transInc.Sum = SumDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Sum_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            bool notified = false;
            transInc.Sum = SumInitialValue;
            transInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransInc.Sum)) notified = true;
            };

            //Act
            transInc.Sum = SumInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}
