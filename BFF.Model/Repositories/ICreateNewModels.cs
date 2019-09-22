using System;
using BFF.Core.IoC;
using BFF.Model.Models;

namespace BFF.Model.Repositories
{
    public interface ICreateNewModels : IOncePerBackend
    {
        ITransaction CreateTransaction();
        ITransfer CreateTransfer();
        IParentTransaction CreateParentTransfer();
        ISubTransaction CreateSubTransaction();
        IAccount CreateAccount();
        IPayee CreatePayee();
        ICategory CreateCategory();
        IIncomeCategory CreateIncomeCategory();
        IFlag CreateFlag();
        IDbSetting CreateDbSetting();
        IBudgetEntry CreateBudgetEntry(DateTime month, ICategory category, long budget, long outflow, long balance);
    }
}
