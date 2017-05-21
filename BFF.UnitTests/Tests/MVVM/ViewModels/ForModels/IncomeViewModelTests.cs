using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class IncomeViewModelTests : TransIncViewModelTests<IncomeViewModel>
    {
        protected override (IncomeViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            IIncome incomeMock = IncomeMoq.Naked;
            incomeMock.Id.Returns(modelId);
            return (new IncomeViewModel(incomeMock, orm), incomeMock);
        }

        protected override (IncomeViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IIncome incomeMock = IncomeMoq.Naked;
                incomeMock.Memo.Returns(MemoInitialValue);
                return (new IncomeViewModel(incomeMock, BffOrmMoq.Naked), incomeMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (IncomeViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IIncome incomeMock = IncomeMoq.Naked;
                incomeMock.Cleared.Returns(ClearedInitialValue);
                incomeMock.Date.Returns(DateInitialValue);
                return (new IncomeViewModel(incomeMock, BffOrmMoq.Naked), incomeMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (IncomeViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IIncome incomeMock = IncomeMoq.Naked;
                incomeMock.AccountId.Returns(c => AccountInitialValue.Id);
                incomeMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new IncomeViewModel(incomeMock, BffOrmMoq.Naked), incomeMock);
            }
        }
        protected override IAccountViewModel AccountInitialValue {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(23);
                return accountViewModel;
            }
        }
        protected override IAccountViewModel AccountDifferentValue {
            get
            {
                var accountViewModel = Substitute.For<IAccountViewModel>();
                accountViewModel.Id.Returns(69);
                return accountViewModel;
            }
        }
        protected override IPayeeViewModel PayeeInitialValue {
            get
            {
                var payeeViewModel = Substitute.For<IPayeeViewModel>();
                payeeViewModel.Id.Returns(23);
                return payeeViewModel;
            }
        }
        protected override IPayeeViewModel PayeeDifferentValue {
            get
            {
                var payeeViewModel = Substitute.For<IPayeeViewModel>();
                payeeViewModel.Id.Returns(69);
                return payeeViewModel;
            }
        }

        protected override (IncomeViewModel, ITransInc) TransIncViewModelFactory
        {
            get
            {
                IIncome incomeMock = IncomeMoq.Naked;
                incomeMock.CategoryId.Returns(c => CategoryInitialValue.Id);
                incomeMock.Sum.Returns(c => SumInitialValue);
                return (new IncomeViewModel(incomeMock, BffOrmMoq.Naked), incomeMock);
            }
        }
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
        protected override long SumInitialValue => 23;
        protected override long SumDifferentValue => 69;
    }
}
