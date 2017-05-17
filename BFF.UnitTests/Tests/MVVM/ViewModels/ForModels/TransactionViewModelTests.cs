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
    public class TransactionViewModelTests : TitLikeViewModelTests<TransactionViewModel>
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
    }
}
