using BFF.Model.Import;
using BFF.Model.Import.Models;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Helper;
using BFF.Persistence.Import;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MrMeeseeks.DataStructures;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.ORM
{
    internal class DapperImportingOrm : IImporter
    {
        private readonly IProvideSqliteConnection _provideConnection;
        private readonly ICrudOrm<IAccountSql> _accountCrudOrm;
        private readonly ICrudOrm<IBudgetEntrySql> _budgetEntryCrudOrm;
        private readonly ICrudOrm<ICategorySql> _categoryCrudOrm;
        private readonly ICrudOrm<IFlagSql> _flagCrudOrm;
        private readonly ICrudOrm<IPayeeSql> _payeeCrudOrm;
        private readonly ICrudOrm<ISubTransactionSql> _subTransactionCrudOrm;
        private readonly ICrudOrm<ITransSql> _transCrudOrm;

        public DapperImportingOrm(
            IProvideSqliteConnection provideConnection,
            ICrudOrm<IAccountSql> accountCrudOrm,
            ICrudOrm<IBudgetEntrySql> budgetEntryCrudOrm,
            ICrudOrm<ICategorySql> categoryCrudOrm,
            ICrudOrm<IFlagSql> flagCrudOrm,
            ICrudOrm<IPayeeSql> payeeCrudOrm,
            ICrudOrm<ISubTransactionSql> subTransactionCrudOrm,
            ICrudOrm<ITransSql> transCrudOrm)
        {
            _provideConnection = provideConnection;
            _accountCrudOrm = accountCrudOrm;
            _budgetEntryCrudOrm = budgetEntryCrudOrm;
            _categoryCrudOrm = categoryCrudOrm;
            _flagCrudOrm = flagCrudOrm;
            _payeeCrudOrm = payeeCrudOrm;
            _subTransactionCrudOrm = subTransactionCrudOrm;
            _transCrudOrm = transCrudOrm;
        }

        public async Task<DtoImportContainer> Import()
        {
            using TransactionScope transactionScope = new (new TransactionScopeOption(), TimeSpan.FromMinutes(10));
            using IDbConnection connection = _provideConnection.Connection;

            var ret = new DtoImportContainer();

            var accounts = (await _accountCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var flags = (await _flagCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var payees = (await _payeeCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var categories = (await _categoryCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var budgetEntries = (await _budgetEntryCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var trans = (await _transCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();
            var subTransactions = (await _subTransactionCrudOrm.ReadAllAsync().ConfigureAwait(false)).ToList();

            transactionScope.Complete();
            transactionScope.Dispose();
            connection.Dispose();

            var accountsIdDictionary = accounts.ToDictionary(account => account.Id, account => account.Name);
            var flagsIdDictionary = flags.ToDictionary(flag => flag.Id, flag => (flag.Name, Color: flag.Color.ToColor()));
            var payeesIdDictionary = payees.ToDictionary(payee => payee.Id, payee => payee.Name);
            var categoryIdDictionary = categories
                .ToDictionary(
                    c => c.Id, 
                    c => (
                        Sql: c, 
                        Dto: new CategoryDto
                        {
                            Name = c.Name,
                            IsIncomeRelevant = c.IsIncomeRelevant,
                            IncomeMonthOffset = c.IsIncomeRelevant ? c.MonthOffset : 0
                        }));

            var categoryIdGrouping = categories
                .Where(c => c.IsIncomeRelevant.Not())
                .GroupBy(c => c.ParentId ?? -1, c => c.Id)
                .ToDictionary(g => g.Key, g => g.ToReadOnlyList());
            var categoryTrees = categoryIdGrouping[-1]
                .SelectTree<long, Tree<CategoryDto>>(
                    id => categoryIdGrouping[id],
                    (id, children) => new Tree<CategoryDto>(categoryIdDictionary[id].Dto, children))
                .ToReadOnlyList();
            
            var subTransactionGrouping = subTransactions
                .GroupBy(st => st.ParentId)
                .ToDictionary(g => g.Key, g => g.ToReadOnlyList());

            ret.AccountStartingBalances = accounts.ToDictionary(account => account.Name, account => account.StartingBalance);
            ret.AccountStartingDates = accounts.ToDictionary(account => account.Name, account => account.StartingDate);
            ret.Categories = Forest<CategoryDto>.CreateFromTrees(categoryTrees);
            ret.IncomeCategories = categoryIdDictionary
                .Values
                .Where(t => t.Dto.IsIncomeRelevant)
                .Select(t => t.Dto)
                .ToList();
            ret.BudgetEntries = budgetEntries
                .Select(be => new BudgetEntryDto
                {
                    Budget = be.Budget,
                    MonthIndex = be.Month.ToMonthIndex(),
                    Category = be.CategoryId is { } id
                        ? categoryIdDictionary[id].Dto
                        : throw new Exception("Impossibruh")
                })
                .ToReadOnlyList();
            ret.Transfers = trans
                .Where(t => t.Type == nameof(TransType.Transfer))
                .Select(t => new TransferDto
                {
                    Date = t.Date,
                    FromAccount = t.PayeeId is {} payeeId ? accountsIdDictionary[payeeId] : "",
                    ToAccount = t.CategoryId is {} categoryId ? accountsIdDictionary[categoryId] : "",
                    CheckNumber = t.CheckNumber,
                    Flag = t.FlagId is {} id ? flagsIdDictionary[id] : ((string Name, Color Color)?)null,
                    Memo = t.Memo,
                    Sum = t.Sum,
                    Cleared = t.Cleared == 1
                })
                .ToReadOnlyList();
            ret.Transactions = trans
                .Where(t => t.Type == nameof(TransType.Transaction))
                .Select(t => new TransactionDto
                {
                    Date = t.Date,
                    Account = t.AccountId is {} accountId ? accountsIdDictionary[accountId] : "",
                    Payee = t.PayeeId is {} payeeId ? payeesIdDictionary[payeeId] : "",
                    Category = t.CategoryId is {} categoryId ? categoryIdDictionary[categoryId].Dto : null,
                    CheckNumber = t.CheckNumber,
                    Flag = t.FlagId is {} id ? flagsIdDictionary[id] : ((string Name, Color Color)?)null,
                    Memo = t.Memo,
                    Sum = t.Sum,
                    Cleared = t.Cleared == 1
                })
                .ToReadOnlyList();
            ret.ParentTransactions = trans
                .Where(t => t.Type == nameof(TransType.ParentTransaction))
                .Select(t => new ParentTransactionDto
                {
                    Date = t.Date,
                    Account = t.AccountId is {} accountId ? accountsIdDictionary[accountId] : "",
                    Payee = t.PayeeId is {} payeeId ? payeesIdDictionary[payeeId] : "",
                    CheckNumber = t.CheckNumber,
                    Flag = t.FlagId is {} id ? flagsIdDictionary[id] : ((string Name, Color Color)?)null,
                    Memo = t.Memo,
                    Cleared = t.Cleared == 1,
                    SubTransactions = subTransactionGrouping[t.Id]
                        .Select(st => new SubTransactionDto
                        {
                            Category = st.CategoryId is {} categoryId ? categoryIdDictionary[categoryId].Dto : null,
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
