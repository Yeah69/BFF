using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class SubIncomeViewModelTests : SubTransIncViewModelTests<SubIncomeViewModel>
    {
        protected override (SubIncomeViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            ISubIncome subIncomeMock = SubIncomeMoq.Naked;
            subIncomeMock.Id.Returns(modelId);
            return (new SubIncomeViewModel(subIncomeMock, Substitute.For<IParentTransIncViewModel>(), orm), subIncomeMock);
        }

        protected override (SubIncomeViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                ISubIncome subIncomeMock = SubIncomeMoq.Naked;
                subIncomeMock.Memo.Returns(MemoInitialValue);
                return (new SubIncomeViewModel(subIncomeMock, Substitute.For<IParentTransIncViewModel>(), BffOrmMoq.Naked), subIncomeMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";

        protected override (SubIncomeViewModel, ISubTransInc) SubTransIncViewModelFactory
        {
            get
            {
                ISubIncome subIncomeMock = SubIncomeMoq.Naked;
                subIncomeMock.Sum.Returns(SumInitialValue);
                subIncomeMock.CategoryId.Returns(c => CategoryInitialValue.Id);
                return (new SubIncomeViewModel(subIncomeMock, Substitute.For<IParentTransIncViewModel>(), BffOrmMoq.Naked), subIncomeMock);
            }
        }
        protected override long SumInitialValue => 23;
        protected override long SumDifferentValue => 69;
        protected override ICategoryViewModel CategoryInitialValue
        {
            get
            {
                var categoryViewModel = Substitute.For<ICategoryViewModel>();
                categoryViewModel.Id.Returns(23);
                return categoryViewModel;
            }
        }
        protected override ICategoryViewModel CategoryDifferentValue
        {
            get
            {
                var categoryViewModel = Substitute.For<ICategoryViewModel>();
                categoryViewModel.Id.Returns(69);
                return categoryViewModel;
            }
        }

        public new static IEnumerable<object[]> AtLeastOneNullCommonPropertyProvider
            => new[]
            {
                new object [] {CommonPropertyProviderMoq.NullCategoryViewModel}
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
