using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;

namespace BFF.DB
{
    public interface IBffOrm : IDisposable
    {
        ICommonPropertyProvider CommonPropertyProvider { get; }
        IBffRepository BffRepository { get; }
        IParentTransactionViewModelService ParentTransactionViewModelService { get; }
        ISubTransactionViewModelService SubTransactionViewModelService { get; }
        IBudgetEntryViewModelService BudgetEntryViewModelService { get; }

        long? GetAccountBalanceUntilNow(IAccount account);
        long? GetSummaryAccountBalanceUntilNow();
        long? GetAccountBalance(IAccount account);
        long? GetSummaryAccountBalance();
        IEnumerable<ISubTransaction> GetSubTransInc(long parentId);
    }
}
