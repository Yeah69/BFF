using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class CommonPropertyProviderMoq
    {
        public static ICommonPropertyProvider Mock => CreateMock();

        internal static ICommonPropertyProvider CreateMock(IList<IAccountViewModel> accountViewModelMocks = null,
                                                                IList<ICategoryViewModel> categoryVieModelMocks = null,
                                                                IList<IPayeeViewModel> payeeViewModelMocks = null,
                                                                ISummaryAccountViewModel summaryAccountViewModelMock = null, 
                                                                IList<IAccount> accountMocks = null, 
                                                                IList<IPayee> payees = null, 
                                                                IList<ICategory> category = null)
        {
            ICommonPropertyProvider mock = Substitute.For<ICommonPropertyProvider>();

            if(accountMocks != null)
            {
                mock.Accounts.Returns(new ObservableCollection<IAccount>(accountMocks));
            }

            if(accountViewModelMocks != null)
            {
                foreach (IAccountViewModel accountViewModel in accountViewModelMocks)
                {
                    mock.GetAccountViewModel(accountViewModel.Id).Returns(accountViewModel);
                }
            }

            if(categoryVieModelMocks != null)
            {
                foreach (ICategoryViewModel categoryViewModel in categoryVieModelMocks)
                {
                    mock.GetCategoryViewModel(categoryViewModel.Id).Returns(categoryViewModel);
                }
            }

            if(payeeViewModelMocks != null)
            {
                foreach (IPayeeViewModel payeeViewModel in payeeViewModelMocks)
                {
                    mock.GetPayeeViewModel(payeeViewModel.Id).Returns(payeeViewModel);
                }
            }

            if(summaryAccountViewModelMock != null)
                mock.SummaryAccountViewModel.Returns(summaryAccountViewModelMock);

            return mock;
        }
    }
}
