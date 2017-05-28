﻿using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class TransactionViewModelTests : TransIncViewModelTests<TransactionViewModel>
    {
        protected override (TransactionViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            ITransaction transactionMock = Substitute.For<ITransaction>();
            transactionMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new TransactionViewModel(transactionMock, bffOrm), transactionMock, bffOrm);
        }

        protected override (TransactionViewModel, ITitLike) TitLikeViewModelFactory {
            get
            {
                ITransaction transactionMock = Substitute.For<ITransaction>();
                transactionMock.Memo.Returns(MemoInitialValue);
                return (new TransactionViewModel(transactionMock, Substitute.For<IBffOrm>()), transactionMock);
            }
        }
        protected override string MemoInitialValue => "Hello";
        protected override string MemoDifferentValue => "World";
        protected override (TransactionViewModel, ITitBase) TitBaseViewModelFactory
        {
            get
            {
                ITransaction transactionMock = Substitute.For<ITransaction>();
                transactionMock.Cleared.Returns(ClearedInitialValue);
                transactionMock.Date.Returns(DateInitialValue);
                return (new TransactionViewModel(transactionMock, Substitute.For<IBffOrm>()), transactionMock);
            }
        }
        protected override bool ClearedInitialValue => true;
        protected override bool ClearedDifferentValue => false;
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override (TransactionViewModel, ITransIncBase) TransIncBaseViewModelFactory {
            get
            {
                ITransaction transactionMock = Substitute.For<ITransaction>();
                transactionMock.AccountId.Returns(c => AccountInitialValue.Id);
                transactionMock.PayeeId.Returns(c => PayeeInitialValue.Id);
                return (new TransactionViewModel(transactionMock, Substitute.For<IBffOrm>()), transactionMock);
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

        protected override (TransactionViewModel, ITransInc) TransIncViewModelFactory
        {
            get
            {
                ITransaction transactionMock = Substitute.For<ITransaction>();
                transactionMock.CategoryId.Returns(c => CategoryInitialValue.Id);
                transactionMock.Sum.Returns(c => SumInitialValue);
                return (new TransactionViewModel(transactionMock, Substitute.For<IBffOrm>()), transactionMock);
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