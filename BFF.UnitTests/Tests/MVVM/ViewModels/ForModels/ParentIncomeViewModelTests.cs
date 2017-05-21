using System;
using System.Collections.Generic;
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
    public class ParentIncomeViewModelTests : ParentTransIncViewModelTests<ParentIncomeViewModel, SubIncome>
    {
        protected override (ParentIncomeViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            IParentIncome parentIncomeMock = ParentIncomeMoq.Naked;
            parentIncomeMock.Id.Returns(modelId);
            return (new ParentIncomeViewModel(parentIncomeMock, orm), parentIncomeMock);
        }

        protected override (ParentIncomeViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IParentIncome parentIncomeMock = ParentIncomeMoq.Naked;
                parentIncomeMock.Memo.Returns(MemoInitialValue);
                return (new ParentIncomeViewModel(parentIncomeMock, BffOrmMoq.Naked), parentIncomeMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (ParentIncomeViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IParentIncome parentIncomeMock = ParentIncomeMoq.Naked;
                parentIncomeMock.Cleared.Returns(ClearedInitialValue);
                parentIncomeMock.Date.Returns(DateInitialValue);
                return (new ParentIncomeViewModel(parentIncomeMock, BffOrmMoq.Naked), parentIncomeMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (ParentIncomeViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IParentIncome parentIncomeMock = ParentIncomeMoq.Naked;
                parentIncomeMock.AccountId.Returns(c => AccountInitialValue.Id);
                parentIncomeMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new ParentIncomeViewModel(parentIncomeMock, BffOrmMoq.Naked), parentIncomeMock);
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

        protected override (ParentIncomeViewModel, IBffOrm) ParentTransIncViewModelFactory {
            get
            {
                var bffOrm = Substitute.For<IBffOrm>();
                bffOrm.GetSubTransInc<SubIncome>(0).Returns(c => new List<SubIncome>());
                return (new ParentIncomeViewModel(new ParentIncome(new DateTime(1969, 6, 9)), bffOrm), bffOrm);
            }
        }
    }
}
