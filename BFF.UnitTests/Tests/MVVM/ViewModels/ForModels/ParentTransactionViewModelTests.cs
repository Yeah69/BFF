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
    public class ParentTransactionViewModelTests : ParentTransIncViewModelTests<ParentTransactionViewModel, SubTransaction>
    {
        protected override (ParentTransactionViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            IParentTransaction parentTransactionMock = ParentTransactionMoq.Naked;
            parentTransactionMock.Id.Returns(modelId);
            return (new ParentTransactionViewModel(parentTransactionMock, orm), parentTransactionMock);
        }

        protected override (ParentTransactionViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                IParentTransaction parentTransactionMock = ParentTransactionMoq.Naked;
                parentTransactionMock.Memo.Returns(MemoInitialValue);
                return (new ParentTransactionViewModel(parentTransactionMock, BffOrmMoq.Naked), parentTransactionMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (ParentTransactionViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                IParentTransaction parentTransactionMock = ParentTransactionMoq.Naked;
                parentTransactionMock.Cleared.Returns(ClearedInitialValue);
                parentTransactionMock.Date.Returns(DateInitialValue);
                return (new ParentTransactionViewModel(parentTransactionMock, BffOrmMoq.Naked), parentTransactionMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (ParentTransactionViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                IParentTransaction parentTransactionMock = ParentTransactionMoq.Naked;
                parentTransactionMock.AccountId.Returns(c => AccountInitialValue.Id);
                parentTransactionMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new ParentTransactionViewModel(parentTransactionMock, BffOrmMoq.Naked), parentTransactionMock);
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
