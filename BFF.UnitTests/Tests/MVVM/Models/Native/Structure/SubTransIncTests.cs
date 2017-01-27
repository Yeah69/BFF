using BFF.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class SubTransIncTests<T> : TitLikeTests<T> where T : SubTransInc
    {
        protected abstract T SubTransIncFactory { get; }

        protected abstract long ParentIdInitialValue { get; }
        protected abstract long ParentIdDifferentValue { get; }

        [Fact]
        public void ParentId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.ParentId = ParentIdInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.ParentId)) notified = true;
            };

            //Act
            subTransInc.ParentId = ParentIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void ParentId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.ParentId = ParentIdInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.ParentId)) notified = true;
            };

            //Act
            subTransInc.ParentId = ParentIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        protected abstract long CategoryIdInitialValue { get; }
        protected abstract long CategoryIdDifferentValue { get; }

        [Fact]
        public void CategoryId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.CategoryId = CategoryIdInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.CategoryId)) notified = true;
            };

            //Act
            subTransInc.CategoryId = CategoryIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void CategoryId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.CategoryId = CategoryIdInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.CategoryId)) notified = true;
            };

            //Act
            subTransInc.CategoryId = CategoryIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void Sum_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.Sum = SumInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.Sum)) notified = true;
            };

            //Act
            subTransInc.Sum = SumDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Sum_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            bool notified = false;
            subTransInc.Sum = SumInitialValue;
            subTransInc.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ISubTransInc.Sum)) notified = true;
            };

            //Act
            subTransInc.Sum = SumInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}
