using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper.Import;
using Category = BFF.DB.PersistenceModels.Category;
using SubTransaction = BFF.DB.PersistenceModels.SubTransaction;

namespace BFF.DB
{
    public interface ICrudOrm : IOncePerBackend
    {
        void Create<T>(T model) where T : class, IPersistenceModel;
        void Create<T>(IEnumerable<T> models) where T : class, IPersistenceModel;

        T Read<T>(long id) where T : class, IPersistenceModel;
        IEnumerable<T> ReadAll<T>() where T : class, IPersistenceModel;

        void Update<T>(T model) where T : class, IPersistenceModel;
        void Update<T>(IEnumerable<T> models) where T : class, IPersistenceModel;

        void Delete<T>(T model) where T : class, IPersistenceModel;
        void Delete<T>(IEnumerable<T> models) where T : class, IPersistenceModel;
    }

    public interface IAccountOrm : IOncePerBackend
    {
        long? GetBalance(long id);
        long? GetBalanceUntilNow(long id);
        long? GetOverallBalance();
        long? GetOverallBalanceUntilNow();
    }

    public interface ICategoryOrm : IOncePerBackend
    {
        IEnumerable<Category> ReadCategories();
        IEnumerable<Category> ReadIncomeCategories();
    }

    public interface IParentalOrm : IOncePerBackend
    {
        IEnumerable<SubTransaction> ReadSubTransactionsOf(long parentTransactionId);
    }

    public interface ITransOrm : IOncePerBackend
    {
        IEnumerable<Trans> GetPageFromSpecificAccount(int offset, int pageSize, long accountId);
        IEnumerable<Trans> GetPageFromSummaryAccount(int offset, int pageSize);

        long GetCountFromSpecificAccount(long accountId);
        long GetCountFromSummaryAccount();

        Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }

    public interface IBudgetOrm : IOncePerBackend
    {
        (IDictionary<DateTime, IList<(BudgetEntry Entry, long Outflow, long Balance)>> BudgetEntriesPerMonth, long InitialNotBudgetedOrOverbudgeted, IDictionary<DateTime, long> IncomesPerMonth, IDictionary<DateTime, long> DanglingTransfersPerMonth) 
            Find(DateTime fromMonth, DateTime toMonth, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories);
    }

    public interface IImportingOrm
    {
        void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments);
    }

    public interface ICreateBackendOrm
    {
        void Create();
    }
}