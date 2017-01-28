using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class CommonPropertyTests<T> : DataModelBaseTests<T> where T : CommonProperty
    {
        protected abstract T CommonPropertyFactory { get; }

        protected abstract string NameInitialValue { get; }
        protected abstract string NameDifferentValue { get; }
        protected abstract string ToStringExpectedValue { get; }

        [Fact]
        public void Name_ChangeValue_TriggersNotification()
        {
            //Arrange
            T commonProperty = CommonPropertyFactory;
            commonProperty.Name = NameInitialValue;

            //Act
            Action shouldTriggerNotification = () => commonProperty.Name = NameDifferentValue;

            //Assert
            Assert.PropertyChanged(commonProperty, nameof(commonProperty.Name), shouldTriggerNotification);
        }

        [Fact]
        public void Name_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T commonProperty = CommonPropertyFactory;
            commonProperty.Name = NameInitialValue;

            //Act
            Action shouldTriggerNotification = () => commonProperty.Name = NameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(commonProperty, nameof(commonProperty.Name), shouldTriggerNotification);
        }

        [Fact]
        public void ToString_ReturnsExpectedValue()
        {
            //Arrange
            T commonProperty = CommonPropertyFactory;

            //Act
            var toStringValue = commonProperty.ToString();

            //Assert
            Assert.Equal(ToStringExpectedValue, toStringValue);
        }
    }
}
