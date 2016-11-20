using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using Moq;

namespace BFF.Tests.Mocks.DB
{
    public static class CommonPropertyProviderMoq
    {
        public static Mock<ICommonPropertyProvider> Mock => CreateMock();

        internal static Mock<ICommonPropertyProvider> CreateMock(IList<Mock<IAccountViewModel>> accountViewModelMocks = null,
                                                                IList<Mock<ICategoryViewModel>> categoryVieModelMocks = null,
                                                                IList<Mock<IPayeeViewModel>> payeeViewModelMocks = null,
                                                                Mock<ISummaryAccountViewModel> summaryAccountViewModelMock = null)
        {
            Mock<ICommonPropertyProvider> mock = new Mock<ICommonPropertyProvider>();

            mock.Setup(cpp => cpp.Add(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<IAccount>())).Verifiable();

            mock.Setup(cpp => cpp.Add(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<ICategory>())).Verifiable();

            mock.Setup(cpp => cpp.Add(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(cpp => cpp.Remove(It.IsAny<IPayee>())).Verifiable();

            if(accountViewModelMocks != null)
            {
                foreach (IAccountViewModel accountViewModel in accountViewModelMocks.Select(avmm => avmm.Object))
                {
                    mock.Setup(cpp => cpp.GetAccountViewModel(accountViewModel.Id)).Returns(accountViewModel);
                }
            }

            if(categoryVieModelMocks != null)
            {
                foreach (ICategoryViewModel categoryViewModel in categoryVieModelMocks.Select(cvmm => cvmm.Object))
                {
                    mock.Setup(cpp => cpp.GetCategoryViewModel(categoryViewModel.Id)).Returns(categoryViewModel);
                }
            }

            if(payeeViewModelMocks != null)
            {
                foreach (IPayeeViewModel payeeViewModel in payeeViewModelMocks.Select(pvmm => pvmm.Object))
                {
                    mock.Setup(cpp => cpp.GetPayeeViewModel(payeeViewModel.Id)).Returns(payeeViewModel);
                }
            }

            if(summaryAccountViewModelMock != null)
                mock.SetupGet(cpp => cpp.SummaryAccountViewModel).Returns(summaryAccountViewModelMock.Object);

            return mock;
        }
    }
}
