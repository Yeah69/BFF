using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class TransactionViewModelTests : DataModelViewModelTests<TransactionViewModel>
    {
        protected override (TransactionViewModel, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId)
        {
            ITransaction transactionMock = TransactionMoq.Naked;
            transactionMock.Id.Returns(modelId);
            return (new TransactionViewModel(transactionMock, orm), transactionMock);
        }
    }
}
