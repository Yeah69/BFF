using BFF.Core.IoC;
using BFF.Model.Import;
using BFF.Model.Import.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Persistence.Helper;
using BFF.Persistence.Import;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using MoreLinq.Extensions;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using BudgetEntry = BFF.Persistence.Realm.Models.Persistence.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;
using Flag = BFF.Persistence.Realm.Models.Persistence.Flag;
using Payee = BFF.Persistence.Realm.Models.Persistence.Payee;
using SubTransaction = BFF.Persistence.Realm.Models.Persistence.SubTransaction;

namespace BFF.Persistence.Realm
{
    internal interface IRealmExporter
    {
        Task Export(DtoImportContainer container);
    }

    internal class RealmExporter : IRealmExporter, IScopeInstance
    {
        private readonly RealmExportingOrm _realmExportingOrm;

        public RealmExporter(
            RealmExportingOrm realmExportingOrm)
        {
            _realmExportingOrm = realmExportingOrm;
        }

        public Task Export(DtoImportContainer container)
        {
            return _realmExportingOrm.PopulateDatabaseAsync(ToRealmExportContainer(container));
        }

        private RealmExportContainer ToRealmExportContainer(DtoImportContainer container)
        {
            var accountDictionary = container
                .Transactions
                .Select(t => t.Account)
                .Concat(container.ParentTransactions.Select(pt => pt.Account))
                .Concat(container.Transfers.SelectMany(t => new[] {t.FromAccount, t.ToAccount}))
                .Distinct()
                .ToDictionary(
                    Basic.Identity,
                    accountName => new Account
                    {
                        Name = accountName,
                        StartingBalance = container.AccountStartingBalances[accountName],
                        StartingDate = ToDateTimeOffset(container.AccountStartingDates[accountName]),
                        StartingMonthIndex = container.AccountStartingDates[accountName].ToMonthIndex()
                    })
                .ToReadOnlyDictionary();

            var payeeDictionary = container
                .Transactions
                .Select(t => t.Payee)
                .Concat(container.ParentTransactions.Select(pt => pt.Payee))
                .Distinct()
                .ToDictionary(
                    Basic.Identity,
                    payeeName => new Payee
                    {
                        Name = payeeName
                    })
                .ToReadOnlyDictionary();

            var flagDictionary = container
                .Transactions
                .Select(t => t.Flag)
                .Concat(container.ParentTransactions.Select(pt => pt.Flag))
                .Concat(container.Transfers.Select(t => t.Flag))
                .Distinct()
                .WhereNotNullable()
                .ToDictionary(
                    Basic.Identity,
                    f => new Flag
                    {
                        Name = f.Name,
                        Color = f.Color.ToLong()
                    })
                .ToReadOnlyDictionary();

            var realmCategoryId = 0;
            var categoryDictionary = new Dictionary<CategoryDto, Category>();

            var categories = container
                .Categories
                .Select<Category>((categoryDto, resultChildren) =>
                {
                    var category = new Category
                    {
                        Id = realmCategoryId++,
                        Name = categoryDto.Name,
                        IsIncomeRelevant = false,
                        IncomeMonthOffset = default,
                    };
                    foreach (var child in resultChildren)
                    {
                        child.Parent = category;
                    }
                    categoryDictionary[categoryDto] = category;
                    return category;
                })
                .Trees
                .IterateTreeDepthFirst(t => t.Branches)
                .Select(t => t.Value)
                .ToReadOnlyList();

            var incomeCategories = container
                .IncomeCategories
                .Select(
                    categoryDto =>
                    {
                        var category = new Category
                        {
                            Id = realmCategoryId++,
                            Name = categoryDto.Name,
                            Parent = null,
                            IsIncomeRelevant = true,
                            IncomeMonthOffset = categoryDto.IncomeMonthOffset,
                        };
                        categoryDictionary[categoryDto] = category;
                        return category;
                    })
                .ToReadOnlyList();

            var realmTransId = 0;
            var subTransactionDtoToRealmParentDictionary = new Dictionary<SubTransactionDto, Trans>();

            var transList =
                container
                    .Transactions
                    .Select(transactionDto =>
                        new Trans
                        {
                            Id = realmTransId++,
                            Account = accountDictionary[transactionDto.Account],
                            Date = ToDateTimeOffset(transactionDto.Date),
                            MonthIndex = transactionDto.Date.ToMonthIndex(),
                            Payee = payeeDictionary[transactionDto.Payee],
                            Category = transactionDto.Category is {} category ? categoryDictionary[category] : null,
                            Flag = transactionDto.Flag is {} flag ? flagDictionary[flag] : null,
                            CheckNumber = transactionDto.CheckNumber,
                            Memo = transactionDto.Memo,
                            Sum = transactionDto.Sum,
                            Cleared = transactionDto.Cleared,
                            TypeIndex = (int) TransType.Transaction,
                            ToAccount = default,
                            FromAccount = default
                        })
                    .Concat(
                        container
                            .Transfers
                            .Select(transferDto =>
                                new Trans
                                {
                                    Id = realmTransId++,
                                    FromAccount = accountDictionary[transferDto.FromAccount],
                                    ToAccount = accountDictionary[transferDto.ToAccount],
                                    Date = ToDateTimeOffset(transferDto.Date),
                                    MonthIndex = ToDateTimeOffset(transferDto.Date).ToMonthIndex(),
                                    Flag = transferDto.Flag is {} flag ? flagDictionary[flag] : null,
                                    CheckNumber = transferDto.CheckNumber,
                                    Memo = transferDto.Memo,
                                    Sum = transferDto.Sum,
                                    Cleared = transferDto.Cleared,
                                    TypeIndex = (int) TransType.Transfer,
                                    Account = default,
                                    Payee = default,
                                    Category = default
                                }))
                    .Concat(
                        container
                            .ParentTransactions
                            .Select(parentTransactionDto =>
                            {
                                var parentTransaction = new Trans
                                {
                                    Id = realmTransId++,
                                    Account = accountDictionary[parentTransactionDto.Account],
                                    Date = ToDateTimeOffset(parentTransactionDto.Date),
                                    MonthIndex = ToDateTimeOffset(parentTransactionDto.Date).ToMonthIndex(),
                                    Payee = payeeDictionary[parentTransactionDto.Payee],
                                    Flag = parentTransactionDto.Flag is {} flag ? flagDictionary[flag] : null,
                                    CheckNumber = parentTransactionDto.CheckNumber,
                                    Memo = parentTransactionDto.Memo,
                                    Cleared = parentTransactionDto.Cleared,
                                    TypeIndex = (int) TransType.ParentTransaction,
                                    ToAccount = default,
                                    FromAccount = default,
                                    Category = default,
                                    Sum = default
                                };
                                parentTransactionDto.SubTransactions.ForEach(subTransactionDto =>
                                    subTransactionDtoToRealmParentDictionary[subTransactionDto] = parentTransaction);
                                return parentTransaction;
                            }))
                    .ToReadOnlyList();

            var realmSubTransactionId = 0;
            var subTransactions =
                container
                    .ParentTransactions
                    .SelectMany(pt => pt.SubTransactions ?? Enumerable.Empty<SubTransactionDto>())
                    .Select(subTransactionDto =>
                        new SubTransaction
                        {
                            Id = realmSubTransactionId++,
                            Parent = subTransactionDtoToRealmParentDictionary[subTransactionDto],
                            Category = subTransactionDto.Category is {} category ? categoryDictionary[category] : null,
                            Memo = subTransactionDto.Memo,
                            Sum = subTransactionDto.Sum
                        })
                    .ToReadOnlyList();

            var realmBudgetEntryId = 0;
            var budgetEntries =
                container
                    .BudgetEntries
                    .Where(be => be.Budget != 0)
                    .Select(budgetEntryDto =>
                        new BudgetEntry
                        {
                            Id = realmBudgetEntryId++,
                            Category = budgetEntryDto.Category is {} category ? categoryDictionary[category] : null,
                            MonthIndex = budgetEntryDto.MonthIndex,
                            Budget = budgetEntryDto.Budget
                        })
                    .ToReadOnlyList();

            return new RealmExportContainer
            {
                Accounts = accountDictionary.Values.ToReadOnlyList(),
                Payees = payeeDictionary.Values.ToReadOnlyList(),
                Categories = categories,
                IncomeCategories = incomeCategories,
                Flags = flagDictionary.Values.ToReadOnlyList(),
                Trans = transList,
                SubTransactions = subTransactions,
                BudgetEntries = budgetEntries
            };

            static DateTimeOffset ToDateTimeOffset(DateTime dateTime) =>
                new (
                    dateTime.Year, 
                    dateTime.Month, 
                    dateTime.Day, 
                    dateTime.Hour, 
                    dateTime.Minute, 
                    dateTime.Second, 
                    dateTime.Millisecond, 
                    TimeSpan.Zero);
        }

        private class RealmExportContainer : IRealmExportContainerData
        {
            public IReadOnlyList<Account> Accounts { get; set; } = new Account[0];
            public IReadOnlyList<Payee> Payees { get; set; } = new Payee[0];
            public IReadOnlyList<Category> Categories { get; set; } = new Category[0];
            public IReadOnlyList<Category> IncomeCategories { get; set; } = new Category[0];
            public IReadOnlyList<Flag> Flags { get; set; } = new Flag[0];
            public IReadOnlyList<Trans> Trans { get; set; } = new Trans[0];
            public IReadOnlyList<SubTransaction> SubTransactions { get; set; } = new SubTransaction[0];
            public IReadOnlyList<BudgetEntry> BudgetEntries { get; set; } = new BudgetEntry[0];
        }
    }
}
