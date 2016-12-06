using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Mocks.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class CommonPropertyProviderMoq
    {
        public static ICommonPropertyProvider Mock => CreateMock();

        internal static ICommonPropertyProvider CreateMock(IList<IAccount> accountMocks = null,
                                                           IList<IPayee> payeeMocks = null,
                                                           IList<ICategory> categoryMocks = null,
                                                           IList<IAccountViewModel> accountViewModelMocks = null,
                                                           IList<ICategoryViewModel> categoryVieModelMocks = null,
                                                           IList<IPayeeViewModel> payeeViewModelMocks = null,
                                                           ISummaryAccountViewModel summaryAccountViewModelMock = null)
        {
            ICommonPropertyProvider mock = Substitute.For<ICommonPropertyProvider>();

            if(accountMocks == null) accountMocks = AccountMoq.Mocks;
            if(payeeMocks == null) payeeMocks = PayeeMoq.Mocks;
            if(categoryMocks == null) categoryMocks = CategoryMoq.Mocks;
            if(accountViewModelMocks == null) accountViewModelMocks = AccountViewModelMoq.Mocks;
            if(payeeViewModelMocks == null) payeeViewModelMocks = PayeeViewModelMoq.Mocks;
            if(categoryVieModelMocks == null) categoryVieModelMocks = CategoryViewModelMoq.Mocks;
            if(summaryAccountViewModelMock == null) summaryAccountViewModelMock = SummaryAccountViewModelMoq.Mock;

            
            mock.Accounts.Returns(new ObservableCollection<IAccount>(accountMocks));
            mock.Payees.Returns(new ObservableCollection<IPayee>(payeeMocks));
            mock.Categories.Returns(new ObservableCollection<ICategory>(categoryMocks));

            foreach (IAccountViewModel accountViewModel in accountViewModelMocks)
            {
                mock.GetAccountViewModel(accountViewModel.Id).Returns(accountViewModel);
            }
            
            foreach (ICategoryViewModel categoryViewModel in categoryVieModelMocks)
            {
                mock.GetCategoryViewModel(categoryViewModel.Id).Returns(categoryViewModel);
            }
            
            foreach (IPayeeViewModel payeeViewModel in payeeViewModelMocks)
            {
                mock.GetPayeeViewModel(payeeViewModel.Id).Returns(payeeViewModel);
            }

            mock.AllPayeeViewModels.Returns(new ObservableCollection<IPayeeViewModel>(payeeViewModelMocks));
            
            mock.SummaryAccountViewModel.Returns(summaryAccountViewModelMock);

            return mock;
        }
    }
}
