using System.Diagnostics;
using BFF.DB.PersistenceModels;

namespace BFF.DB.SQLite
{
    internal class MonthCategoryRequestDto
    {
        public long CategoryId { get; }
        public string Month { get; }
        public string Year { get; }

        public MonthCategoryRequestDto(long categoryId, string month, string year)
        {
            CategoryId = categoryId;
            Month = month;
            Year = year;
        }
    }
    
    internal class MonthOutflowBudgetResponseDto
    {
        public long? Id { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public long? Outflow { get; set; }
        public long? Budget { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Month)}: {Month}, {nameof(Year)}: {Year}, {nameof(Outflow)}: {Outflow}, {nameof(Budget)}: {Budget}";
        }
    }

    internal static class SqLiteQueries
    {
        internal static readonly int DatabaseSchemaVersion = 2;

        internal static string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";

        private static string BoolExpressionUntilMonth(string monthPropertyName) =>
            $@"strftime('%Y', {monthPropertyName}) < @{nameof(MonthCategoryRequestDto.Year)}
        OR strftime('%m', {monthPropertyName}) <= @{nameof(MonthCategoryRequestDto.Month)}
        AND strftime('%Y', {monthPropertyName}) = @{nameof(MonthCategoryRequestDto.Year)}";






        private static string BoolExpressionMonth(string monthPropertyName) =>
            $@"strftime('%m', {monthPropertyName}) = @{nameof(MonthCategoryRequestDto.Month)}
        AND strftime('%Y', {monthPropertyName}) = @{nameof(MonthCategoryRequestDto.Year)}";






        private static string WhereClauseCategoryAndUntilMonth(string monthPropertyName) =>
            $@"WHERE {nameof(Transaction.CategoryId)} = @{nameof(MonthCategoryRequestDto.CategoryId)}
        AND ({BoolExpressionUntilMonth(monthPropertyName)})";





        private static readonly string _selectBudgetEntriesForCategoryUntilMonth =
            $@"SELECT strftime('%Y', {nameof(BudgetEntry.Month)}) AS {nameof(MonthOutflowBudgetResponseDto.Year)}, strftime('%m', {nameof(BudgetEntry.Month)}) AS {nameof(MonthOutflowBudgetResponseDto.Month)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Budget)}, {nameof(BudgetEntry.Id)}
  FROM {nameof(BudgetEntry)}s
  {WhereClauseCategoryAndUntilMonth(nameof(BudgetEntry.Month))}";





        private static readonly string _selectDateCategorySumFromAllExternalTits =
            $@"SELECT {nameof(Transaction.Date)}, {nameof(Transaction.CategoryId)}, {nameof(Transaction.Sum)}
    FROM {nameof(Transaction)}s
    UNION ALL
    SELECT {nameof(Income.Date)}, {nameof(Income.CategoryId)}, {nameof(Income.Sum)}
    FROM {nameof(Income)}s
    UNION ALL
    SELECT {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Date)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}
      FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(ParentTransaction)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Id)}
    UNION ALL
    SELECT {nameof(ParentIncome)}s.{nameof(ParentIncome.Date)}, {nameof(SubIncome)}s.{nameof(SubIncome.CategoryId)}, {nameof(SubIncome)}s.{nameof(SubIncome.Sum)}
      FROM {nameof(SubIncome)}s
        INNER JOIN {nameof(ParentIncome)}s ON {nameof(SubIncome)}s.{nameof(SubIncome.ParentId)} = {nameof(ParentIncome)}s.{nameof(ParentIncome.Id)}";





        private static readonly string _selectYearMonthCategoryTotalSumFromAllExternalTits =
            $@"SELECT strftime('%Y', {nameof(Transaction.Date)}) AS {nameof(MonthOutflowBudgetResponseDto.Year)}, strftime('%m', {nameof(Transaction.Date)}) AS {nameof(MonthOutflowBudgetResponseDto.Month)}, {nameof(Transaction.CategoryId)}, SUM({nameof(Transaction.Sum)}) AS {nameof(MonthOutflowBudgetResponseDto.Outflow)}
  FROM
  (
    {_selectDateCategorySumFromAllExternalTits}
  )
  {WhereClauseCategoryAndUntilMonth(nameof(Transaction.Date))}
  GROUP BY strftime('%Y', {nameof(Transaction.Date)}), strftime('%m', {nameof(Transaction.Date)}), {nameof(Transaction.CategoryId)}";




        internal static string SelectBudgetOutflowData =
            $@"SELECT * FROM
(SELECT {nameof(MonthOutflowBudgetResponseDto.Year)}, {nameof(MonthOutflowBudgetResponseDto.Month)}, {nameof(MonthOutflowBudgetResponseDto.Budget)}, {nameof(MonthOutflowBudgetResponseDto.Outflow)}, {nameof(MonthOutflowBudgetResponseDto.Id)}
FROM
(
    {_selectYearMonthCategoryTotalSumFromAllExternalTits}
)
INNER JOIN (
  {_selectBudgetEntriesForCategoryUntilMonth}
  ) USING ({nameof(MonthCategoryRequestDto.Year)}, {nameof(MonthCategoryRequestDto.Month)}, {nameof(MonthCategoryRequestDto.CategoryId)})

UNION ALL
SELECT {nameof(MonthOutflowBudgetResponseDto.Year)}, {nameof(MonthOutflowBudgetResponseDto.Month)}, {nameof(MonthOutflowBudgetResponseDto.Budget)}, {nameof(MonthOutflowBudgetResponseDto.Outflow)}, {nameof(MonthOutflowBudgetResponseDto.Id)}
FROM
(
    {_selectYearMonthCategoryTotalSumFromAllExternalTits}
)
LEFT OUTER JOIN (
  {_selectBudgetEntriesForCategoryUntilMonth}
  ) r USING ({nameof(MonthCategoryRequestDto.Year)}, {nameof(MonthCategoryRequestDto.Month)}, {nameof(MonthCategoryRequestDto.CategoryId)})
WHERE r.Year IS NULL

UNION ALL
SELECT {nameof(MonthOutflowBudgetResponseDto.Year)}, {nameof(MonthOutflowBudgetResponseDto.Month)}, {nameof(MonthOutflowBudgetResponseDto.Budget)}, {nameof(MonthOutflowBudgetResponseDto.Outflow)}, {nameof(MonthOutflowBudgetResponseDto.Id)}
FROM
(
  {_selectBudgetEntriesForCategoryUntilMonth}
)
LEFT OUTER JOIN (
    {_selectYearMonthCategoryTotalSumFromAllExternalTits}
  ) r USING ({nameof(MonthCategoryRequestDto.Year)}, {nameof(MonthCategoryRequestDto.Month)}, {nameof(MonthCategoryRequestDto.CategoryId)})
WHERE r.Year IS NULL)
ORDER BY {nameof(MonthOutflowBudgetResponseDto.Year)}, {nameof(MonthOutflowBudgetResponseDto.Month)};";
    }
}
