using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class SubTransactionViewModelTests : SubTransIncViewModelTests<SubTransactionViewModel>
    {
        protected override (SubTransactionViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            ISubTransaction subTransactionMock = Substitute.For<ISubTransaction>();
            subTransactionMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new SubTransactionViewModel(subTransactionMock, Substitute.For<IParentTransIncViewModel>(), bffOrm), subTransactionMock, bffOrm);
        }

        protected override (SubTransactionViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                ISubTransaction subTransactionMock = Substitute.For<ISubTransaction>();
                subTransactionMock.Memo.Returns(MemoInitialValue);
                return (new SubTransactionViewModel(subTransactionMock, Substitute.For<IParentTransIncViewModel>(), Substitute.For<IBffOrm>()), subTransactionMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";

        protected override (SubTransactionViewModel, ISubTransInc) SubTransIncViewModelFactory
        {
            get
            {
                ISubTransaction subTransactionMock = Substitute.For<ISubTransaction>();
                subTransactionMock.Sum.Returns(SumInitialValue);
                subTransactionMock.CategoryId.Returns(c => CategoryInitialValue.Id);
                return (new SubTransactionViewModel(subTransactionMock, Substitute.For<IParentTransIncViewModel>(), Substitute.For<IBffOrm>()), subTransactionMock);
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
