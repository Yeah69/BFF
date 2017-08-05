using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class SubTransactionViewModelService : ModelToViewModelServiceBase<ISubTransaction, ISubTransactionViewModel>
    {
        private readonly Func<ISubTransaction, ISubTransactionViewModel> _subTransactionViewModelFactory;

        public SubTransactionViewModelService(
            Func<ISubTransaction, ISubTransactionViewModel> subTransactionViewModelFactory)
        {
            _subTransactionViewModelFactory = subTransactionViewModelFactory;
        }

        protected override ISubTransactionViewModel Create(ISubTransaction model)
        {
            return _subTransactionViewModelFactory(model);
        }
    }
}
