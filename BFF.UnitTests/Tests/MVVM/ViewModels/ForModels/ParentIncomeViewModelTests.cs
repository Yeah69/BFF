using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class ParentIncomeViewModelTests : ParentTransIncViewModelTests<ParentIncomeViewModel, SubIncome>
    {
        protected override (ParentIncomeViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            IParentIncome parentIncomeMock = Substitute.For<IParentIncome>();
            parentIncomeMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new ParentIncomeViewModel(parentIncomeMock, bffOrm), parentIncomeMock, bffOrm);
        }

        protected override (ParentIncomeViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IParentIncome parentIncomeMock = Substitute.For<IParentIncome>();
                parentIncomeMock.Memo.Returns(MemoInitialValue);
                return (new ParentIncomeViewModel(parentIncomeMock, Substitute.For<IBffOrm>()), parentIncomeMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (ParentIncomeViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IParentIncome parentIncomeMock = Substitute.For<IParentIncome>();
                parentIncomeMock.Cleared.Returns(ClearedInitialValue);
                parentIncomeMock.Date.Returns(DateInitialValue);
                return (new ParentIncomeViewModel(parentIncomeMock, Substitute.For<IBffOrm>()), parentIncomeMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (ParentIncomeViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IParentIncome parentIncomeMock = Substitute.For<IParentIncome>();
                parentIncomeMock.AccountId.Returns(c => AccountInitialValue.Id);
                parentIncomeMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new ParentIncomeViewModel(parentIncomeMock, Substitute.For<IBffOrm>()), parentIncomeMock);
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
