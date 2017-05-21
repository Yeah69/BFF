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
    public class TransactionViewModelTests : TransIncBaseViewModelTests<TransactionViewModel>
    {
        protected override (TransactionViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            ITransaction transactionMock = TransactionMoq.Naked;
            transactionMock.Id.Returns(modelId);
            return (new TransactionViewModel(transactionMock, orm), transactionMock);
        }

        protected override (TransactionViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                ITransaction transactionMock = TransactionMoq.Naked;
                transactionMock.Memo.Returns(MemoInitialValue);
                return (new TransactionViewModel(transactionMock, BffOrmMoq.Naked), transactionMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (TransactionViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                ITransaction transactionMock = TransactionMoq.Naked;
                transactionMock.Cleared.Returns(ClearedInitialValue);
                transactionMock.Date.Returns(DateInitialValue);
                return (new TransactionViewModel(transactionMock, BffOrmMoq.Naked), transactionMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (TransactionViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                ITransaction transactionMock = TransactionMoq.Naked;
                transactionMock.AccountId.Returns(c => AccountInitialValue.Id);
                transactionMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new TransactionViewModel(transactionMock, BffOrmMoq.Naked), transactionMock);
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
    }
}
