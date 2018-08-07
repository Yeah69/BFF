using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using Category = BFF.DB.PersistenceModels.Category;
using Flag = BFF.DB.PersistenceModels.Flag;
using Payee = BFF.DB.PersistenceModels.Payee;
using SubTransaction = BFF.DB.PersistenceModels.SubTransaction;

namespace BFF.DB
{
    public interface ICrudOrm : IOncePerBackend
    {
        Task CreateAsync<T>(T model) where T : class, IPersistenceModel;
        Task CreateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel;

        Task<T> ReadAsync<T>(long id) where T : class, IPersistenceModel;
        Task<IEnumerable<T>> ReadAllAsync<T>() where T : class, IPersistenceModel;

        Task UpdateAsync<T>(T model) where T : class, IPersistenceModel;
        Task UpdateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel;

        Task DeleteAsync<T>(T model) where T : class, IPersistenceModel;
        Task DeleteAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel;
    }
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(Payee from, Payee to);
        Task MergeFlagAsync(Flag from, Flag to);
        Task MergeCategoryAsync(Category from, Category to);
    }

    public interface IAccountOrm : IOncePerBackend
    {
        Task<long?> GetClearedBalanceAsync(long id);
        Task<long?> GetClearedBalanceUntilNowAsync(long id);
        Task<long?> GetClearedOverallBalanceAsync();
        Task<long?> GetClearedOverallBalanceUntilNowAsync();
        Task<long?> GetUnclearedBalanceAsync(long id);
        Task<long?> GetUnclearedBalanceUntilNowAsync(long id);
        Task<long?> GetUnclearedOverallBalanceAsync();
        Task<long?> GetUnclearedOverallBalanceUntilNowAsync();
    }

    public interface ICategoryOrm : IOncePerBackend
    {
        Task<IEnumerable<Category>> ReadCategoriesAsync();
        Task<IEnumerable<Category>> ReadIncomeCategoriesAsync();
    }

    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<SubTransaction>> ReadSubTransactionsOfAsync(long parentTransactionId);
    }

    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<Trans>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }

    public interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories);
    }

    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(ImportLists importLists, ImportAssignments importAssignments);
    }

    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}