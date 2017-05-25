using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class DataModelTests<T> where T : DataModel
    {
        protected abstract T DataModelBaseFactory { get; }

        protected abstract long IdInitialValue { get; }
        protected abstract long IdDifferentValue { get; }

        [Fact]
        public void Id_ChangeValue_TriggersNotification()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            dataModelBase.Id = IdInitialValue;

            //Act
            Action shouldTriggerNotification = () => dataModelBase.Id = IdDifferentValue;

            //Assert
            Assert.PropertyChanged(dataModelBase, nameof(dataModelBase.Id), shouldTriggerNotification);
        }

        [Fact]
        public void Id_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            dataModelBase.Id = IdInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dataModelBase.Id = IdInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dataModelBase, nameof(dataModelBase.Id), shouldNotTriggerNotification);
        }

        [Fact]
        public void Insert_Orm_CallsInsertOnOrm()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            IBffOrm ormMock = Substitute.For<IBffOrm>();

            //Act
            dataModelBase.Insert(ormMock);

            //Assert
            ormMock.Received().Insert(Arg.Any<T>());
        }

        [Fact]
        public void Insert_Null_ThrowsArgumentNullException()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;

            //Act + Assert
            Assert.Throws<ArgumentNullException>(() => dataModelBase.Insert(null));
        }

        [Fact]
        public void Update_Orm_CallsUpdateOnOrm()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            IBffOrm ormMock = Substitute.For<IBffOrm>();

            //Act
            dataModelBase.Update(ormMock);

            //Assert
            ormMock.Received().Update(Arg.Any<T>());
        }

        [Fact]
        public void Update_Null_ThrowsArgumentNullException()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;

            //Act + Assert
            Assert.Throws<ArgumentNullException>(() => dataModelBase.Update(null));
        }

        [Fact]
        public void Delete_Orm_CallsUpdateOnOrm()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            IBffOrm ormMock = Substitute.For<IBffOrm>();

            //Act
            dataModelBase.Delete(ormMock);

            //Assert
            ormMock.Received().Delete(Arg.Any<T>());
        }

        [Fact]
        public void Delete_Null_ThrowsArgumentNullException()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;

            //Act + Assert
            Assert.Throws<ArgumentNullException>(() => dataModelBase.Delete(null));
        }
    }
}
