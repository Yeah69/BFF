using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.DB.SQLite;
using Dapper;
using MoreLinq;
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
        IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime fromMonth, DateTime toMonth, Domain.ICategory category, DbConnection connection);
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

        public IList<Domain.IBudgetEntry> GetBudgetEntries(DateTime fromMonth, DateTime toMonth, Domain.ICategory category, DbConnection connection)
        {
            void FillPlaceholderBudgetEntries(
                ICollection<Domain.IBudgetEntry> c, 
                int fromY, 
                int fromM, 
                int y,
                int m,
                long b)
            {
                int currentYear = fromY;
                int currentMonth = fromM;
                do
                {
                    c.Add(new Domain.BudgetEntry(
                        this,
                        -1L,
                        new DateTime(currentYear, currentMonth, 1),
                        category,
                        0L,
                        0L,
                        b));

                    if (currentMonth != 12)
                    {
                        currentMonth = currentMonth + 1;
                    }
                    else
                    {
                        currentYear = currentYear + 1;
                        currentMonth = 1;
                    }
                } while (currentYear <= y && currentMonth != m);
            }

            fromMonth = new DateTime(2017, 7, 1);
            toMonth = new DateTime(2017, 12, 1);
            category = new Domain.Category(null, 49, "", null);

            var complementedResponses =
                // Fetch the budget responses from the database
                ConnectionHelper.QueryOnExistingOrNewConnection(
                c =>
                {
                    var entries = c.Query<MonthOutflowBudgetResponseDto>(
                        SqLiteQueries.SelectBudgetOutflowData,
                        new MonthCategoryRequestDto(category.Id, $"{toMonth.Month:00}", $"{toMonth.Year:0000}"));
                    return entries;
                }, ProvideConnection, connection)

                // Complement responses with balance
                .Scan(
                (new MonthOutflowBudgetResponseDto
                {
                    Month = "11",
                    Year = "0000",
                    Budget = 0L,
                    Outflow = 0L
                },
                0L, 
                new MonthOutflowBudgetResponseDto
                {
                    Month = "12",
                    Year = "0000",
                    Budget = 0L,
                    Outflow = 0L
                },
                0L),
                (previousTwoResults, currentResponse) =>
                {
                    (_ , _, var previousResponse , var previousBalance) = previousTwoResults;
                    return (
                    previousResponse, 
                    previousBalance, 
                    currentResponse, 
                    Math.Max(0L, previousBalance + (currentResponse.Budget ?? 0L) - (currentResponse.Outflow ?? 0L)));
                })

                // Skip all responses before fromMonth
                .SkipWhile(responseAndBalance =>
                {
                    (_, _, var response, _) = responseAndBalance;
                    int year = int.Parse(response.Year);
                    int month = int.Parse(response.Month);
                    return year < fromMonth.Year || month < fromMonth.Month;
                })

                // Select BudgetEntries from fromMonth until last retrieved month
                .SelectMany(responseAndBalance =>
                {
                    (var previousResponse, var previousBalance, var response, var balance) = responseAndBalance;

                    int previousYear = int.Parse(previousResponse.Year);
                    int previousMonth = int.Parse(previousResponse.Month);
                    int year = int.Parse(response.Year);
                    int month = int.Parse(response.Month);

                    if(previousYear == year && previousMonth + 1 == month || // same year, one month apart
                    previousYear + 1 == year && previousMonth == 12 && month == 1) // consecutive years, one month apart
                        return new[] { new Domain.BudgetEntry(
                            this,
                            response.Id ?? -1L,
                            new DateTime(int.Parse(response.Year), int.Parse(response.Month), 1),
                            category,
                            response.Budget ?? 0L,
                            response.Outflow ?? 0L,
                            balance)};

                    // From this point the two responses are more than a month apart
                    int currentYear;
                    int currentMonth;
                    if (previousYear < fromMonth.Year || previousMonth < fromMonth.Month)
                    {
                        currentYear = fromMonth.Year;
                        currentMonth = fromMonth.Month;
                    }
                    else if (previousMonth != 12)
                    {
                        currentYear = previousYear;
                        currentMonth = previousMonth + 1;
                    }
                    else
                    {
                        currentYear = previousYear + 1;
                        currentMonth = 1;
                    }

                    ICollection<Domain.IBudgetEntry> collection = new Collection<Domain.IBudgetEntry>();
                    FillPlaceholderBudgetEntries(
                        collection,
                        currentYear,
                        currentMonth,
                        month != 1 ? year : year - 1,
                        month != 1 ? month - 1 : 12,
                        previousBalance);

                    collection.Add(new Domain.BudgetEntry(
                        this,
                        response.Id ?? -1L,
                        new DateTime(year, month, 1),
                        category,
                        response.Budget ?? 0L,
                        response.Outflow ?? 0L,
                        balance));
                    return collection;
                }).ToList();

            var last = complementedResponses.Last();

            if(last.Month.Year < toMonth.Year || last.Month.Month < toMonth.Month)
                FillPlaceholderBudgetEntries(
                    complementedResponses,
                    last.Month.Month != 12 ? last.Month.Year : last.Month.Year + 1,
                    last.Month.Month != 12 ? last.Month.Month + 1 : 1,
                    toMonth.Year,
                    toMonth.Month,
                    last.Balance);

            return null;
        }
    }
}