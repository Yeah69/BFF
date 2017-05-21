using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    public abstract class ParentTransIncViewModelTests<T, TSub> : TransIncBaseViewModelTests<T> 
        where T : ParentTransIncViewModel where TSub : ISubTransInc
    {
        protected abstract (T, IBffOrm) ParentTransIncViewModelFactory { get; }

        [Fact]
        public void SubElementsGet_CallsOrmForSubElements()
        {
            //Arrange
            (T viewModel, IBffOrm mock) = ParentTransIncViewModelFactory;

            //Act
            _ = viewModel.SubElements;

            //Assert
            mock.Received().GetSubTransInc<TSub>(Arg.Any<long>());
        }

        [Fact]
        public void SumGet_CallsOrmForSubElements()
        {
            //Arrange
            (T viewModel, IBffOrm mock) = ParentTransIncViewModelFactory;

            //Act
            _ = viewModel.Sum;

            //Assert
            mock.Received().GetSubTransInc<TSub>(Arg.Any<long>());
        }

        public static IEnumerable<object[]> AtLeastOneNullCommonPropertyProvider
            => new[]
            {
                new object [] {CommonPropertyProviderMoq.NullAccountViewModel},
                new object [] {CommonPropertyProviderMoq.NullPayeeViewModel}
            };

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public override void Insert_AtLeastOneNullCommonPropertyProvider_NotInserted_DoesntCallInsertOnOrm(ICommonPropertyProvider commonPropertyProvider)
        {
            base.Insert_AtLeastOneNullCommonPropertyProvider_NotInserted_DoesntCallInsertOnOrm(commonPropertyProvider);
        }

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public override void ValidToInsert_AtLeastOneNullCommonPropertyProvider_NotInserted_False(ICommonPropertyProvider commonPropertyProvider)
        {
            base.ValidToInsert_AtLeastOneNullCommonPropertyProvider_NotInserted_False(commonPropertyProvider);
        }
    }
}