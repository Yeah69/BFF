using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                                                           IList<ICategoryViewModel> categoryViewModelMocks = null,
                                                           IList<IPayeeViewModel> payeeViewModelMocks = null,
                                                           ISummaryAccountViewModel summaryAccountViewModelMock = null)
        {
            ICommonPropertyProvider mock = Substitute.For<ICommonPropertyProvider>();

            if(accountMocks == null) accountMocks = AccountMoq.Mocks;
            if(payeeMocks == null) payeeMocks = PayeeMoq.Mocks;
            if(categoryMocks == null) categoryMocks = CategoryMoq.Mocks;
            if(accountViewModelMocks == null) accountViewModelMocks = AccountViewModelMoq.Mocks;
            if(payeeViewModelMocks == null) payeeViewModelMocks = PayeeViewModelMoq.Mocks;
            if(categoryViewModelMocks == null) categoryViewModelMocks = CategoryViewModelMoq.Mocks;
            if(summaryAccountViewModelMock == null) summaryAccountViewModelMock = SummaryAccountViewModelMoq.Mock;

            
            mock.Accounts.Returns(new ObservableCollection<IAccount>(accountMocks));
            mock.Payees.Returns(new ObservableCollection<IPayee>(payeeMocks));
            mock.Categories.Returns(new ObservableCollection<ICategory>(categoryMocks));

            foreach (IAccountViewModel accountViewModel in accountViewModelMocks)
            {
                mock.GetAccountViewModel(accountViewModel.Id).Returns(accountViewModel);
            }
            
            foreach (ICategoryViewModel categoryViewModel in categoryViewModelMocks)
            {
                mock.GetCategoryViewModel(categoryViewModel.Id).Returns(categoryViewModel);
            }
            
            foreach (IPayeeViewModel payeeViewModel in payeeViewModelMocks)
            {
                mock.GetPayeeViewModel(payeeViewModel.Id).Returns(payeeViewModel);
            }

            mock.AllAccountViewModels.Returns(new ObservableCollection<IAccountViewModel>(accountViewModelMocks));
            mock.AllPayeeViewModels.Returns(new ObservableCollection<IPayeeViewModel>(payeeViewModelMocks));
            mock.AllCategoryViewModels.Returns(new ObservableCollection<ICategoryViewModel>(categoryViewModelMocks));
            ObservableCollection<ICategoryViewModel> parentCategories = new ObservableCollection<ICategoryViewModel>(
                categoryViewModelMocks.Where(cvmm => cvmm.Parent == null));
            mock.ParentCategoryViewModels.Returns(parentCategories);

            mock.SummaryAccountViewModel.Returns(summaryAccountViewModelMock);

            return mock;
        }
    }
}
