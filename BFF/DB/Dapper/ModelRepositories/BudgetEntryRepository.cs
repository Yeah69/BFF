using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.DB.SQLite;
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

    public interface IBudgetEntryRepository : IRepositoryBase<Domain.IBudgetEntry>
    {
        IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime month, DbConnection connection);
    }


    public sealed class BudgetEntryRepository : RepositoryBase<Domain.IBudgetEntry, BudgetEntry>, IBudgetEntryRepository
    {
        private static string MonthQuery =>
            $@"SELECT * FROM [{nameof(BudgetEntry)}s] WHERE strftime('%m', {nameof(BudgetEntry.Month)}) == @Month AND strftime('%Y', {nameof(BudgetEntry.Month)}) == @Year;";

        private static string OutflowQuery =>
            $@"Select SUM(Sum) FROM
(
  SELECT SUM({nameof(Transaction.Sum)}) AS Sum
  FROM {nameof(Transaction)}s
  WHERE {nameof(Transaction.CategoryId)} = @CategoryId
        AND strftime('%m', {nameof(Transaction.Date)}) = @Month
        AND strftime('%Y', {nameof(Transaction.Date)}) = @Year
  UNION ALL
  SELECT SUM({nameof(Income.Sum)}) AS Sum
  FROM {nameof(Income)}s
  WHERE {nameof(Income.CategoryId)} = @CategoryId
        AND strftime('%m', {nameof(Income.Date)}) = @Month
        AND strftime('%Y', {nameof(Income.Date)}) = @Year
  UNION ALL
  SELECT SUM({nameof(SubTransaction.Sum)}) AS Sum FROM
  (
    SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)}, {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Date)}
    FROM {nameof(SubTransaction)}s
      INNER JOIN {nameof(ParentTransaction)}s
        ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Id)}
  )
  WHERE {nameof(SubTransaction.CategoryId)} = @CategoryId
        AND strftime('%m', {nameof(ParentTransaction.Date)}) = @Month
        AND strftime('%Y', {nameof(ParentTransaction.Date)}) = @Year
  UNION ALL
  SELECT SUM({nameof(SubIncome.Sum)}) AS Sum FROM
  (
    SELECT {nameof(SubIncome)}s.{nameof(SubIncome.Sum)}, {nameof(SubIncome)}s.{nameof(SubIncome.CategoryId)}, {nameof(ParentIncome)}s.{nameof(ParentIncome.Date)}
    FROM {nameof(SubIncome)}s
      INNER JOIN {nameof(ParentIncome)}s
        ON {nameof(SubIncome)}s.{nameof(SubIncome.ParentId)} = {nameof(ParentIncome)}s.{nameof(ParentIncome.Id)}
  )
  WHERE {nameof(SubIncome.CategoryId)} = @CategoryId
        AND strftime('%m', {nameof(ParentIncome.Date)}) = @Month
        AND strftime('%Y', {nameof(ParentIncome.Date)}) = @Year
);";

        private static string TotalOutflowQuery =>
            $@"Select SUM(Sum) FROM
(
  SELECT SUM(Sum) AS Sum
  FROM Transactions
  WHERE CategoryId = @CategoryId
        AND (strftime('%Y', Date) < @Year
        OR strftime('%m', Date) <= @Month
        AND strftime('%Y', Date) = @Year)
  UNION ALL
  SELECT SUM(Sum) AS Sum
  FROM Incomes
  WHERE CategoryId = @CategoryId
        AND (strftime('%Y', Date) < @Year
        OR strftime('%m', Date) <= @Month
        AND strftime('%Y', Date) = @Year)
  UNION ALL
  SELECT SUM(Sum) AS Sum FROM
  (
    SELECT SubTransactions.Sum, SubTransactions.CategoryId, ParentTransactions.Date
    FROM SubTransactions
      INNER JOIN ParentTransactions
        ON SubTransactions.ParentId = ParentTransactions.Id
  )
  WHERE CategoryId = @CategoryId
        AND (strftime('%Y', Date) < @Year
        OR strftime('%m', Date) <= @Month
        AND strftime('%Y', Date) = @Year)
  UNION ALL
  SELECT SUM(Sum) AS Sum FROM
  (
    SELECT SubIncomes.Sum, SubIncomes.CategoryId, ParentIncomes.Date
    FROM SubIncomes
      INNER JOIN ParentIncomes
        ON SubIncomes.ParentId = ParentIncomes.Id
  )
  WHERE CategoryId = @CategoryId
        AND (strftime('%Y', Date) < @Year
        OR strftime('%m', Date) <= @Month
        AND strftime('%Y', Date) = @Year)
);";

        private static string TotalBudgetQuery =>
            $@"SELECT SUM(Budget)
FROM BudgetEntrys
WHERE CategoryId = @CategoryId
      AND (strftime('%Y', Month) < @Year
      OR strftime('%m', Month) <= @Month
      AND strftime('%Y', Month) = @Year);";

        private readonly Func<long?, DbConnection, Domain.ICategory> _categoryFetcher;
        public BudgetEntryRepository(IProvideConnection provideConnection, Func<long?, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.IBudgetEntry Create() =>
            new Domain.BudgetEntry(this, -1L, DateTime.MinValue);
        
        protected override Converter<Domain.IBudgetEntry, BudgetEntry> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category?.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        protected override Converter<(BudgetEntry, DbConnection), Domain.IBudgetEntry> ConvertToDomain => tuple =>
        {
            (BudgetEntry persistenceBudgetEntry, DbConnection connection) = tuple;
            return new Domain.BudgetEntry(
                this,
                persistenceBudgetEntry.Id,
                persistenceBudgetEntry.Month,
                _categoryFetcher(persistenceBudgetEntry.CategoryId, connection),
                persistenceBudgetEntry.Budget,
                connection.Query<long?>(
                    OutflowQuery,
                    new
                    {
                        Month = $"{persistenceBudgetEntry.Month.Month:00}",
                        Year = $"{persistenceBudgetEntry.Month.Year:0000}",
                        persistenceBudgetEntry.CategoryId
                    }).First() ?? 0L,
                connection.Query<long?>(
                    TotalOutflowQuery,
                    new
                    {
                        Month = $"{persistenceBudgetEntry.Month.Month:00}",
                        Year = $"{persistenceBudgetEntry.Month.Year:0000}",
                        persistenceBudgetEntry.CategoryId
                    }).First() ?? 0L +
                connection.Query<long?>(
                    TotalBudgetQuery,
                    new
                    {
                        Month = $"{persistenceBudgetEntry.Month.Month:00}",
                        Year = $"{persistenceBudgetEntry.Month.Year:0000}",
                        persistenceBudgetEntry.CategoryId
                    }).First() ?? 0L);
        };

        public IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime month, DbConnection connection)
        {
            month = new DateTime(2017, 1, 1);
            var budgetEntries = ConnectionHelper.QueryOnExistingOrNewConnection(
                c =>
                {
                    var entries = c.Query<MonthOutflowBudgetResponseDto>(
                        SqLiteQueries.SelectBudgetOutflowData,
                        new MonthCategoryRequestDto(49, $"{month.Month:00}", $"{month.Year:0000}"));
                    return entries;
                }, ProvideConnection, connection);
            return null;
        }
    }
}