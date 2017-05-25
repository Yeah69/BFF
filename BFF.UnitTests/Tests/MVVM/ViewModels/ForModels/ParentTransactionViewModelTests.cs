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
    public class ParentTransactionViewModelTests : ParentTransIncViewModelTests<ParentTransactionViewModel, SubTransaction>
    {
        protected override (ParentTransactionViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            IParentTransaction parentTransactionMock = Substitute.For<IParentTransaction>();
            parentTransactionMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new ParentTransactionViewModel(parentTransactionMock, bffOrm), parentTransactionMock, bffOrm);
        }

        protected override (ParentTransactionViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IParentTransaction parentTransactionMock = Substitute.For<IParentTransaction>();
                parentTransactionMock.Memo.Returns(MemoInitialValue);
                return (new ParentTransactionViewModel(parentTransactionMock, Substitute.For<IBffOrm>()), parentTransactionMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (ParentTransactionViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IParentTransaction parentTransactionMock = Substitute.For<IParentTransaction>();
                parentTransactionMock.Cleared.Returns(ClearedInitialValue);
                parentTransactionMock.Date.Returns(DateInitialValue);
                return (new ParentTransactionViewModel(parentTransactionMock, Substitute.For<IBffOrm>()), parentTransactionMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (ParentTransactionViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IParentTransaction parentTransactionMock = Substitute.For<IParentTransaction>();
                parentTransactionMock.AccountId.Returns(c => AccountInitialValue.Id);
                parentTransactionMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new ParentTransactionViewModel(parentTransactionMock, Substitute.For<IBffOrm>()), parentTransactionMock);
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

        protected override (ParentTransactionViewModel, IBffOrm) ParentTransIncViewModelFactory {
            get
            {
                var bffOrm = Substitute.For<IBffOrm>();
                bffOrm.GetSubTransInc<SubTransaction>(0).Returns(c => new List<SubTransaction>());
                return (new ParentTransactionViewModel(new ParentTransaction(new DateTime(1969, 6, 9)), bffOrm), bffOrm);
            }
        }
    }
}
