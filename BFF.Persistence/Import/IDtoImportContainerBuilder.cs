using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BFF.Persistence.Import.Models;
using MrMeeseeks.DataStructures;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Import
{
    internal interface IDtoImportContainerBuilder
    {
        void SetAccountStartingBalance(string name, long balance);

        void TrySetAccountStartingDate(string name, DateTime date);

        void AddAsTransfer(
            DateTime date,
            string fromAccount,
            string toAccount,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            long sum,
            bool cleared);

        void AddAsTransaction(
            DateTime date,
            string account,
            string payee,
            CategoryDto category,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            long sum,
            bool cleared);

        void AddParentAndSubTransactions(
            DateTime date,
            string account,
            string payee,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            bool cleared,
            IEnumerable<(CategoryDto Category, string Memo, long Sum)> subTransactions);

        CategoryDto GetOrCreateCategory(IEnumerable<string> namePath);

        CategoryDto GetOrCreateIncomeCategory(string name, int monthOffset);

        void AddBudgetEntry(
            DateTime month,
            CategoryDto category,
            long budget);

        DtoImportContainer BuildContainer();
    }

    internal class DtoImportContainerBuilder : IDtoImportContainerBuilder
    {
        private readonly IList<TransactionDto> _transactions = new List<TransactionDto>();
        private readonly IList<TransferDto> _transfers = new List<TransferDto>();
        private readonly IList<ParentTransactionDto> _parentTransactions = new List<ParentTransactionDto>();
        private readonly IList<BudgetEntryDto> _budgetEntries = new List<BudgetEntryDto>();
        private readonly IDictionary<string, long> _accountStartingBalances = new Dictionary<string, long>();
        private readonly IDictionary<string, DateTime> _accountStartingDates = new Dictionary<string, DateTime>();
        private readonly Forest<CategoryDto> _categoryForest = new Forest<CategoryDto>();
        private readonly IDictionary<string, CategoryDto> _incomeCategories = new Dictionary<string, CategoryDto>();

        public void SetAccountStartingBalance(string name, long balance)
        {
            _accountStartingBalances[name] = balance;
        }

        public void TrySetAccountStartingDate(string name, DateTime date)
        {
            _accountStartingDates[name] =
                _accountStartingDates.TryGetValue(name, out var currentDate)
                && currentDate < date
                ? currentDate
                : date;
        }

        public void AddAsTransfer(
            DateTime date,
            string fromAccount,
            string toAccount,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            long sum,
            bool cleared)
        {
            _transfers.Add(new TransferDto
            {
                Date = date,
                FromAccount = fromAccount,
                ToAccount = toAccount,
                CheckNumber = checkNumber,
                Flag = flag,
                Memo = memo,
                Sum = sum,
                Cleared = cleared
            });
            TrySetAccountStartingDate(fromAccount, date);
            TrySetAccountStartingDate(toAccount, date);
        }

        public void AddAsTransaction(
            DateTime date,
            string account,
            string payee,
            CategoryDto category,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            long sum,
            bool cleared)
        {
            _transactions.Add(new TransactionDto
            {
                Date = date,
                Account = account,
                Payee = payee,
                Category = category,
                CheckNumber = checkNumber,
                Flag = flag,
                Memo = memo,
                Sum = sum,
                Cleared = cleared
            });
            TrySetAccountStartingDate(account, date);
        }

        public void AddParentAndSubTransactions(
            DateTime date,
            string account,
            string payee,
            string checkNumber,
            (string, Color)? flag,
            string memo,
            bool cleared,
            IEnumerable<(CategoryDto Category, string Memo, long Sum)> subTransactions)
        {
            _parentTransactions.Add(new ParentTransactionDto
            {
                Date = date,
                Account = account,
                Payee = payee,
                CheckNumber = checkNumber,
                Flag = flag,
                Memo = memo,
                Cleared = cleared,
                SubTransactions =
                    subTransactions
                        .Select(t => new SubTransactionDto { Category = t.Category, Memo = t.Memo, Sum = t.Sum})
                        .ToReadOnlyList()
            });
        }

        public CategoryDto GetOrCreateCategory(IEnumerable<string> namePath)
        {
            return _categoryForest.GetOrCreate(namePath, Compare, Create).Value;

            static bool Compare(CategoryDto category, string name) => Equals(category.Name, name);
            static CategoryDto Create(string name) => new CategoryDto { Name = name, IncomeMonthOffset = 0, IsIncomeRelevant = false };
        }

        public CategoryDto GetOrCreateIncomeCategory(string name, int monthOffset)
        {
            if (_incomeCategories.TryGetValue(name, out var ic))
            {
                return ic;
            }

            var incomeCategory = new CategoryDto { Name = name, IsIncomeRelevant = true, IncomeMonthOffset = monthOffset };
            _incomeCategories[name] = incomeCategory;
            return incomeCategory;
        }

        public void AddBudgetEntry(
            DateTime month,
            CategoryDto category,
            long budget)
        {
            _budgetEntries.Add(new BudgetEntryDto
            {
                Month = month,
                Category = category,
                Budget = budget
            });
        }

        public DtoImportContainer BuildContainer()
        {
            return new DtoImportContainer
            {
                Transactions = _transactions.ToReadOnlyList(),
                Transfers = _transfers.ToReadOnlyList(),
                ParentTransactions = _parentTransactions.ToReadOnlyList(),
                BudgetEntries = _budgetEntries.ToReadOnlyList(),
                Categories = _categoryForest,
                IncomeCategories = _incomeCategories.Values.ToReadOnlyList(),
                AccountStartingBalances = _accountStartingBalances.ToReadOnlyDictionary(),
                AccountStartingDates = _accountStartingDates.ToReadOnlyDictionary()
            };
        }
    }
}