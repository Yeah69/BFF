using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Extensions;
using BFF.Persistence.Import;
using BFF.Persistence.Import.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using MoreLinq.Extensions;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;
using Flag = BFF.Persistence.Realm.Models.Persistence.Flag;
using Payee = BFF.Persistence.Realm.Models.Persistence.Payee;

namespace BFF.Persistence.Realm
{
    internal interface IExporter
    {
        Task ExportAsync(DtoImportContainer container);
    }

    internal class RealmExporter : IExporter, IDisposable
    {
        private readonly RealmExportingOrm _realmExportingOrm;
        private readonly IDisposable _disposeOnDisposal;

        public RealmExporter(
            RealmExportingOrm realmExportingOrm,
            IDisposable disposeOnDisposal)
        {
            _realmExportingOrm = realmExportingOrm;
            _disposeOnDisposal = disposeOnDisposal;
        }

        public Task ExportAsync(DtoImportContainer container)
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
                        StartingDate = new DateTimeOffset(container.AccountStartingDates[accountName], TimeSpan.Zero)
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
                .WhereNotNull()
                .ToDictionary(
                    Basic.Identity,
                    f => new Flag
                    {
                        Name = f?.Name,
                        Color = f?.Color.ToLong() 
                                ?? Color.Transparent.ToLong()
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
                        Month = default,
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
                            Month = categoryDto.IncomeMonthOffset,
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
                            Date = new DateTimeOffset(transactionDto.Date, TimeSpan.Zero),
                            Payee = payeeDictionary[transactionDto.Payee],
                            Category = categoryDictionary[transactionDto.Category],
                            Flag = transactionDto.Flag is null ? null : flagDictionary[transactionDto.Flag],
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
                                    Date = new DateTimeOffset(transferDto.Date, TimeSpan.Zero),
                                    Flag = transferDto.Flag is null ? null : flagDictionary[transferDto.Flag],
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
                                    Date = new DateTimeOffset(parentTransactionDto.Date, TimeSpan.Zero),
                                    Payee = payeeDictionary[parentTransactionDto.Payee],
                                    Flag = parentTransactionDto.Flag is null ? null : flagDictionary[parentTransactionDto.Flag],
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
                    .SelectMany(pt => pt.SubTransactions)
                    .Select(subTransactionDto =>
                        new SubTransaction
                        {
                            Id = realmSubTransactionId++,
                            Parent = subTransactionDtoToRealmParentDictionary[subTransactionDto],
                            Category = categoryDictionary[subTransactionDto.Category],
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
                            Category = categoryDictionary[budgetEntryDto.Category],
                            Month = new DateTimeOffset(budgetEntryDto.Month, TimeSpan.Zero),
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
        }

        private class RealmExportContainer : IRealmExportContainerData
        {
            public IReadOnlyList<Account> Accounts { get; set; }
            public IReadOnlyList<Payee> Payees { get; set; }
            public IReadOnlyList<Category> Categories { get; set; }
            public IReadOnlyList<Category> IncomeCategories { get; set; }
            public IReadOnlyList<Flag> Flags { get; set; }
            public IReadOnlyList<Trans> Trans { get; set; }
            public IReadOnlyList<SubTransaction> SubTransactions { get; set; }
            public IReadOnlyList<BudgetEntry> BudgetEntries { get; set; }
        }

        public void Dispose()
        {
            _disposeOnDisposal.Dispose();
        }
    }
}
