using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Helper;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class CategoryTests : CommonPropertyTests<Category>
    {
        protected override Category DataModelBaseFactory => new Category(IdInitialValue, 1, "Food");
        protected override long IdInitialValue => 11;
        protected override long IdDifferentValue => 12;

        protected override Category CommonPropertyFactory => new Category(1, 1, NameInitialValue);
        protected override string NameInitialValue => "Cinema";
        protected override string NameDifferentValue => "Hobby";
        protected override string ToStringExpectedValue => "Cinema";

        Category CategoryFactory => new Category(1, ParentIdInitialValue, "Bank");

        long ParentIdInitialValue => 690;
        long ParentIdDifferentValue => 230;

        [Fact]
        public void ParentId_ChangeValue_TriggersNotification()
        {
            //Arrange
            Category category = CategoryFactory;
            category.ParentId = ParentIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => category.ParentId = ParentIdDifferentValue;

            //Assert
            Assert.PropertyChanged(category, nameof(category.ParentId), shouldTriggerNotification);
        }

        [Fact]
        public void ParentId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            Category category = CategoryFactory;
            category.ParentId = ParentIdInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => category.ParentId = ParentIdInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(category, nameof(category.ParentId), shouldNotTriggerNotification);
        }
    }
}
