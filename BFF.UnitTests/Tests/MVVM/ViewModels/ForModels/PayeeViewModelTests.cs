using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public class PayeeViewModelTests : CommonPropertyViewModelTests<PayeeViewModel>
    {
        protected override (PayeeViewModel, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider = null)
        {
            IPayee payeeMock = Substitute.For<IPayee>();
            payeeMock.Id.Returns(modelId);
            var bffOrm = Substitute.For<IBffOrm>();
            bffOrm.CommonPropertyProvider
                  .Returns(ci => commonPropertyProvider ?? Substitute.For<ICommonPropertyProvider>());
            return (new PayeeViewModel(payeeMock, bffOrm), payeeMock, bffOrm);
        }
        protected override (PayeeViewModel, ICommonProperty) CommonPropertyViewModelFactory
        {
            get
            {
                IPayee payeeMock = Substitute.For<IPayee>();
                payeeMock.Name.Returns(NameInitialValue);
                return (new PayeeViewModel(payeeMock, Substitute.For<IBffOrm>()), payeeMock);
            }
        }

        protected override string NameInitialValue => "Restaurant";
        protected override string NameDifferentValue => "Cinema";
    }
}
