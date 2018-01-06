using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateBudgetEntryTable : CreateTableBase
    {
        public CreateBudgetEntryTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(BudgetEntry)}s](
            {nameof(BudgetEntry.Id)} INTEGER PRIMARY KEY,
            {nameof(BudgetEntry.CategoryId)} INTEGER,
            {nameof(BudgetEntry.Month)} DATE,
            {nameof(BudgetEntry.Budget)} INTEGER,
            FOREIGN KEY({nameof(BudgetEntry.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
    }

    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<Domain.IBudgetEntry>
    {
        IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime fromMonth, DateTime toMonth, Domain.ICategory category, DbConnection connection);
    }


    public sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<Domain.IBudgetEntry, BudgetEntry>, IBudgetEntryRepository
    {
        private readonly ICategoryRepository _categoryRepository;

        private class BudgetResponse
        {
            public DateTime Month { get; set; }
            public long Budget { get; set; }
        }
        private class OutflowResponse
        {
            public DateTime Month { get; set; }
            public long Sum { get; set; }
        }

        private static readonly string BudgetQuery =
$@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(BudgetEntry.Month)}) < @Year OR strftime('%Y', {nameof(BudgetEntry.Month)} = @Year AND strftime('%m', {nameof(BudgetEntry.Month)} <= @Month)))
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string OutflowQuery =
            $@"SELECT Date as Month, Sum(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) = @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month)
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) < @Year OR strftime('%Y', Date) = @Year AND strftime('%m', Date) <= @Month
)
  GROUP BY Date
  ORDER BY Date;";

        public BudgetEntryRepository(IProvideConnection provideConnection, ICategoryRepository categoryRepository ) : base(provideConnection)
        {
            _categoryRepository = categoryRepository;
        }
        
        protected override Converter<Domain.IBudgetEntry, BudgetEntry> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category?.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        public IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime fromMonth, DateTime toMonth, Domain.ICategory category, DbConnection connection)
        {
            var budgetList = ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<BudgetEntry>(
                    BudgetQuery,
                    new
                    {
                        CategoryId = category.Id,
                        Year = $"{toMonth.Year:0000}",
                        Month = $"{toMonth.Month:00}"
                    }), 
                ProvideConnection, 
                connection)
                .ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), be => be);

            var outflowList = ConnectionHelper.QueryOnExistingOrNewConnection(
                    c => c.Query<OutflowResponse>(
                        OutflowQuery,
                        new
                        {
                            CategoryId = category.Id,
                            Year = $"{toMonth.Year:0000}",
                            Month = $"{toMonth.Month:00}"
                        }),
                    ProvideConnection,
                    connection)
                .ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);

            long entryBudgetValue = 0L;

            var previous = budgetList.Keys.Where(dt => dt < fromMonth).Concat(outflowList.Keys.Where(dt => dt < fromMonth)).Distinct().OrderBy(dt => dt).Select(dt =>
            {
                (long Budget, long Outflow) ret = (0L, 0L);
                if (budgetList.ContainsKey(dt))
                    ret.Budget = budgetList[dt].Budget;
                if (outflowList.ContainsKey(dt))
                    ret.Outflow = outflowList[dt];
                return ret;
            });

            foreach (var result in previous)
            {
                entryBudgetValue = Math.Max(0L, entryBudgetValue + result.Budget + result.Outflow);
            }

            var currentDate = new DateTime(fromMonth.Year, fromMonth.Month, 01);

            var budgetEntries = new List<Domain.IBudgetEntry>();

            do
            {
                long id = -1;
                long budgeted = 0L;
                if (budgetList.ContainsKey(currentDate))
                {
                    var budgetEntry = budgetList[currentDate];
                    budgeted = budgetEntry.Budget;
                    id = budgetEntry.Id;
                }
                long outflow = 0L;
                if (outflowList.ContainsKey(currentDate))
                    outflow = outflowList[currentDate];
                long currentValue = entryBudgetValue + budgeted + outflow;
                budgetEntries.Add(new Domain.BudgetEntry(this, id, currentDate, _categoryRepository.Find(category.Id, connection), budgeted, outflow, currentValue));

                entryBudgetValue = Math.Max(0L, currentValue);
                currentDate = new DateTime(
                    currentDate.Month == 12 ? currentDate.Year + 1 : currentDate.Year, 
                    currentDate.Month == 12 ? 1 : currentDate.Month + 1, 
                    1);

            } while (currentDate.Year < toMonth.Year || currentDate.Year == toMonth.Year && currentDate.Month <= toMonth.Month);

            return budgetEntries;
        }
    }
}