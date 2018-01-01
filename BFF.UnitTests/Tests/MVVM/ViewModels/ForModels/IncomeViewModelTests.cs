using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class IncomeViewModelTests : TransIncViewModelTests<IncomeViewModel>
    {
        protected override (IncomeViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            IIncome incomeMock = Substitute.For<IIncome>();
            incomeMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new IncomeViewModel(incomeMock, bffOrm), incomeMock, bffOrm);
        }

        protected override (IncomeViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IIncome incomeMock = Substitute.For<IIncome>();
                incomeMock.Memo.Returns(MemoInitialValue);
                return (new IncomeViewModel(incomeMock, Substitute.For<IBffOrm>()), incomeMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (IncomeViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IIncome incomeMock = Substitute.For<IIncome>();
                incomeMock.Cleared.Returns(ClearedInitialValue);
                incomeMock.Date.Returns(DateInitialValue);
                return (new IncomeViewModel(incomeMock, Substitute.For<IBffOrm>()), incomeMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (IncomeViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IIncome incomeMock = Substitute.For<IIncome>();
                incomeMock.AccountId.Returns(c => AccountInitialValue.Id);
                incomeMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new IncomeViewModel(incomeMock, Substitute.For<IBffOrm>()), incomeMock);
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
                IIncome incomeMock = Substitute.For<IIncome>();
                incomeMock.CategoryId.Returns(c => CategoryInitialValue.Id);
                incomeMock.Sum.Returns(c => SumInitialValue);
                return (new IncomeViewModel(incomeMock, Substitute.For<IBffOrm>()), incomeMock);
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
