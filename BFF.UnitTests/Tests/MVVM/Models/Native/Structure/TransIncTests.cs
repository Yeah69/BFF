using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
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
            transInc.CategoryId = CategoryIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => transInc.CategoryId = CategoryIdDifferentValue;

            //Assert
            Assert.PropertyChanged(transInc, nameof(transInc.CategoryId), shouldTriggerNotification);
        }

        [Fact]
        public void CategoryId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            transInc.CategoryId = CategoryIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => transInc.CategoryId = CategoryIdInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transInc, nameof(transInc.CategoryId), shouldTriggerNotification);
        }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void Sum_ChangeValue_TriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            transInc.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => transInc.Sum = SumDifferentValue;

            //Assert
            Assert.PropertyChanged(transInc, nameof(transInc.Sum), shouldTriggerNotification);
        }

        [Fact]
        public void Sum_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T transInc = TransIncFactory;
            transInc.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => transInc.Sum = SumInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transInc, nameof(transInc.Sum), shouldTriggerNotification);
        }
    }
}
