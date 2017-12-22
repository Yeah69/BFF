using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;

namespace BFF.DB
{
    public interface IBffOrm : IPagedOrm, IDisposable
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
        IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc;
    }

    public interface IPagedOrm
    {
        int GetCount<T>(object specifyingObject = null);
        IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
