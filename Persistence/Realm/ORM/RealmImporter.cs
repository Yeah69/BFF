using BFF.Model.Import;
using BFF.Model.Import.Models;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using MrMeeseeks.DataStructures;
using MrMeeseeks.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;

namespace BFF.Persistence.Realm.ORM
{
    internal interface IRealmImporter
    {
        Task<DtoImportContainer> Import();
    }

    internal class RealmImporter : IRealmImporter
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmImporter(
            IProvideRealmConnection provideConnection) =>
            _provideConnection = provideConnection;

        public async Task<DtoImportContainer> Import()
        {
            await Task.Delay(1).ConfigureAwait(false);
            using TransactionScope transactionScope = new (new TransactionScopeOption(), TimeSpan.FromMinutes(10));
            using Realms.Realm realm = _provideConnection.Connection;

            var ret = new DtoImportContainer();

            var accounts = realm.All<Account>().ToReadOnlyList();
            var categories = realm.All<Category>().ToReadOnlyList();
            var budgetEntries = realm.All<BudgetEntry>().ToReadOnlyList();
            var trans = realm.All<Trans>().ToReadOnlyList();
            var subTransactions = realm.All<SubTransaction>().ToReadOnlyList();
            
            var categoryDictionary = categories
                .ToDictionary(
                    c => c, 
                    c => new CategoryDto
                        {
                            Name = c.Name ?? throw new Exception("Impossible"),
                            IsIncomeRelevant = c.IsIncomeRelevant,
                            IncomeMonthOffset = c.IsIncomeRelevant ? c.IncomeMonthOffset : 0
                        });

            var categoryGrouping = categories
                .Where(c => c.IsIncomeRelevant.Not() && c.Parent is not null)
                .GroupBy(c => c.Parent, c => c)
                .ToDictionary(g => g.Key, g => g.ToReadOnlyList());
            var categoryTrees = categories
                .Where(c => c.IsIncomeRelevant.Not() && c.Parent is null)
                .SelectTree<Category, Tree<CategoryDto>>(
                    category => categoryGrouping.TryGetValue(category, out var children) ? children : new List<Category>(),
                    (category, children) => new Tree<CategoryDto>(categoryDictionary[category], children))
                .ToReadOnlyList();
            
            var subTransactionGrouping = subTransactions
                .GroupBy(st => st.Parent)
                .ToDictionary(g => g.Key, g => g.ToReadOnlyList());

            ret.AccountStartingBalances = accounts.ToDictionary(account => account.Name, account => account.StartingBalance);
            ret.AccountStartingDates = accounts.ToDictionary(account => account.Name, account => account.StartingDate.UtcDateTime);
            ret.Categories = Forest<CategoryDto>.CreateFromTrees(categoryTrees);
            ret.IncomeCategories = categoryDictionary
                .Values
                .Where(t => t.IsIncomeRelevant)
                .Select(t => t)
                .ToList();
            ret.BudgetEntries = budgetEntries
                .Select(be => new BudgetEntryDto
                {
                    Budget = be.Budget,
                    MonthIndex = be.MonthIndex,
                    Category = be.Category is { } category
                        ? categoryDictionary[category]
                        : throw new Exception("Impossibruh")
                })
                .ToReadOnlyList();
            ret.Transfers = trans
                .Where(t => t.TypeIndex == (int) TransType.Transfer)
                .Select(t => new TransferDto
                {
                    Date = t.Date.UtcDateTime,
                    FromAccount = t.FromAccount?.Name ?? "",
                    ToAccount = t.ToAccount?.Name ?? "",
                    CheckNumber = t.CheckNumber ?? "",
                    Flag = t.Flag is {} flag ? (flag.Name, flag.Color.ToColor()) : ((string Name, Color Color)?)null,
                    Memo = t.Memo ?? "",
                    Sum = t.Sum,
                    Cleared = t.Cleared
                })
                .ToReadOnlyList();
            ret.Transactions = trans
                .Where(t => t.TypeIndex == (int) TransType.Transaction)
                .Select(t => new TransactionDto
                {
                    Date = t.Date.UtcDateTime,
                    Account = t.Account?.Name ?? "",
                    Payee = t.Payee?.Name ?? "",
                    Category = t.Category is {} category ? categoryDictionary[category] : null,
                    CheckNumber = t.CheckNumber ?? "",
                    Flag = t.Flag is {} flag ? (flag.Name, flag.Color.ToColor()) : ((string Name, Color Color)?)null,
                    Memo = t.Memo ?? "",
                    Sum = t.Sum,
                    Cleared = t.Cleared
                })
                .ToReadOnlyList();
            ret.ParentTransactions = trans
                .Where(t =>  t.TypeIndex == (int) TransType.ParentTransaction)
                .Select(t => new ParentTransactionDto
                {
                    Date = t.Date.UtcDateTime,
                    Account = t.Account?.Name ?? "",
                    Payee = t.Payee?.Name ?? "",
                    CheckNumber = t.CheckNumber ?? "",
                    Flag = t.Flag is {} flag ? (flag.Name, flag.Color.ToColor()) : ((string Name, Color Color)?)null,
                    Memo = t.Memo ?? "",
                    Cleared = t.Cleared,
                    SubTransactions = subTransactionGrouping[t]
                        .Select(st => new SubTransactionDto
                        {
                            Category = st.Category is {} category ? categoryDictionary[category] : null,
                            Memo = st.Memo,
                            Sum = st.Sum
                        })
                        .ToReadOnlyList()
                })
                .ToReadOnlyList();

            return ret;
        }
    }
}
