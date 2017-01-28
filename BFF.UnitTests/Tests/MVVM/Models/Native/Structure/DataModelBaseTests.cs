using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class DataModelBaseTests<T> where T : DataModelBase
    {
        protected abstract T DataModelBaseFactory { get; }

        protected abstract long IdInitialValue { get; }
        protected abstract long IdDifferentValue { get; }

        [Fact]
        public void Id_ChangeValue_TriggersNotification()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            bool notified = false;
            dataModelBase.Id = IdInitialValue;
            dataModelBase.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == nameof(IDataModelBase.Id)) notified = true;
            };

            //Act
            dataModelBase.Id = IdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Id_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            bool notified = false;
            dataModelBase.Id = IdInitialValue;
            dataModelBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IDataModelBase.Id)) notified = true;
            };

            //Act
            dataModelBase.Id = IdInitialValue;

            //Assert
            Assert.False(notified);
        }

        [Fact]
        public void Insert_Orm_CallsInsertOnOrm()
        {
            //Arrange
            T dataModelBase = DataModelBaseFactory;
            IBffOrm ormMock = BffOrmMoq.NakedFake;

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
            IBffOrm ormMock = BffOrmMoq.NakedFake;

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
            IBffOrm ormMock = BffOrmMoq.NakedFake;

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
