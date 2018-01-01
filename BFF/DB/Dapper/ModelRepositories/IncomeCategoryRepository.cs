using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Dapper;
using Category = BFF.DB.PersistenceModels.Category;
using Trans = BFF.DB.PersistenceModels.Trans;

namespace BFF.DB.Dapper.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<MVVM.Models.Native.IIncomeCategory>
    {
        public override int Compare(MVVM.Models.Native.IIncomeCategory x, MVVM.Models.Native.IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<MVVM.Models.Native.IIncomeCategory>
    {
        long GetMonthsIncome(DateTime month, DbConnection connection);
        long GetIncomeUntilMonth(DateTime month, DbConnection connection);
    }

    public sealed class IncomeCategoryRepository : ObservableRepositoryBase<MVVM.Models.Native.IIncomeCategory, Category>, IIncomeCategoryRepository
    {
        private readonly IProvideConnection _provideConnection;
        private static readonly string GetAllQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 1;";

        private static string IncomeForCategoryQuery(int offset) =>
            $@"
    SELECT Sum({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Month;";

        private static string IncomeForCategoryUntilMonthQuery(int offset) =>
            $@"
    SELECT Sum({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND (strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) < @Year OR strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) <= @Month);";


        public IncomeCategoryRepository(IProvideConnection provideConnection)
            : base(provideConnection, new IncomeCategoryComparer())
        {
            _provideConnection = provideConnection;
        }

        public override MVVM.Models.Native.IIncomeCategory Create() =>
            new MVVM.Models.Native.IncomeCategory(this, -1, "", 0);

        public long GetMonthsIncome(DateTime month, DbConnection connection)
        {
            long incomeSum = 0;
            foreach (var incomeCategory in All)
            {
                incomeSum += ConnectionHelper.QueryOnExistingOrNewConnection(
                    c => c.Query<long?>(IncomeForCategoryQuery(incomeCategory.MonthOffset), new
                    {
                        CategoryId = incomeCategory.Id,
                        Year = $"{month.Year:0000}",
                        Month = $"{month.Month:00}"
                    }),
                    _provideConnection,
                    connection).First() ?? 0L;
            }

            return incomeSum;
        }

        public long GetIncomeUntilMonth(DateTime month, DbConnection connection)
        {

            long incomeSum = 0;
            foreach (var incomeCategory in All)
            {
                incomeSum += ConnectionHelper.QueryOnExistingOrNewConnection(
                    c => c.Query<long?>(IncomeForCategoryUntilMonthQuery(incomeCategory.MonthOffset), new
                    {
                        CategoryId = incomeCategory.Id,
                        Year = $"{month.Year:0000}",
                        Month = $"{month.Month:00}"
                    }),
                    _provideConnection,
                    connection).First() ?? 0L;
            }

            return incomeSum;
        }


        protected override IEnumerable<Category> FindAllInner(DbConnection connection)
        {
            return connection.Query<Category>(GetAllQuery);
        }

        protected override Converter<MVVM.Models.Native.IIncomeCategory, Category> ConvertToPersistence => domainCategory =>
            new Category
            {
                Id = domainCategory.Id,
                Name = domainCategory.Name,
                ParentId = null,
                IsIncomeRelevant = true,
                MonthOffset = domainCategory.MonthOffset
            };

        protected override Converter<(Category, DbConnection), MVVM.Models.Native.IIncomeCategory> ConvertToDomain => tuple =>
        {
            (Category persistenceCategory, DbConnection _) = tuple;
            return new MVVM.Models.Native.IncomeCategory(this,
                persistenceCategory.Id,
                persistenceCategory.Name,
                persistenceCategory.MonthOffset);
        };
    }
}
