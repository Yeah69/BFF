using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Helper;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.ORM
{
    internal class DapperCrudOrm<T> : ICrudOrm<T> 
        where T : class, IPersistenceModelSql
    {
        private static readonly string OnAccountDeletion = $@"
DELETE FROM {nameof(Trans)}s WHERE {nameof(Trans.AccountId)} = @accountId AND ({nameof(Trans.Type)} = '{TransType.Transaction}' OR {nameof(Trans.Type)} = '{TransType.ParentTransaction}');
DELETE FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND ({nameof(Trans.PayeeId)} IS NULL AND {nameof(Trans.CategoryId)} = @accountId OR {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.CategoryId)} IS NULL);
UPDATE {nameof(Trans)}s SET {nameof(Trans.PayeeId)} = NULL WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.CategoryId)} IS NOT NULL;
UPDATE {nameof(Trans)}s SET {nameof(Trans.CategoryId)} = NULL WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.PayeeId)} IS NOT NULL;";
        
        private static readonly string OnCategoryDeletion = $@"
UPDATE {nameof(Trans)}s SET {nameof(Trans.CategoryId)} = NULL WHERE {nameof(Trans.Type)} = '{TransType.Transaction}' AND {nameof(Trans.CategoryId)} = @categoryId;
UPDATE {nameof(SubTransaction)}s SET {nameof(SubTransaction.CategoryId)} = NULL WHERE {nameof(SubTransaction.CategoryId)} = @categoryId;";

        private static readonly string OnPayeeDeletion = $@"
UPDATE {nameof(Trans)}s SET {nameof(Trans.PayeeId)} = NULL WHERE {nameof(Trans.Type)} = '{TransType.Transaction}' AND {nameof(Trans.PayeeId)} = @payeeId;
UPDATE {nameof(Trans)}s SET {nameof(Trans.PayeeId)} = NULL WHERE {nameof(Trans.Type)} = '{TransType.ParentTransaction}' AND {nameof(Trans.PayeeId)} = @payeeId;";

        private readonly IProvideSqliteConnection _provideConnection;

        public DapperCrudOrm(IProvideSqliteConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<long> CreateAsync(T model)
        {
            if (model.Id > 0) return model.Id;
            using TransactionScope transactionScope = new TransactionScope();
            using IDbConnection connection = _provideConnection.Connection;

            long id = model switch
            {
                IAccountSql account => await connection.InsertAsync(account as Account).ConfigureAwait(false),
                IBudgetEntrySql budgetEntry => await connection.InsertAsync(budgetEntry as BudgetEntry)
                    .ConfigureAwait(false),
                ICategorySql category => await connection.InsertAsync(category as Category).ConfigureAwait(false),
                IDbSettingSql dbSetting => await connection.InsertAsync(dbSetting as DbSetting).ConfigureAwait(false),
                IFlagSql flag => await connection.InsertAsync(flag as Flag).ConfigureAwait(false),
                IPayeeSql payee => await connection.InsertAsync(payee as Payee).ConfigureAwait(false),
                ISubTransactionSql subTransaction => await connection.InsertAsync(subTransaction as SubTransaction)
                    .ConfigureAwait(false),
                ITransSql trans => await connection.InsertAsync(trans as Trans).ConfigureAwait(false),
                _ => throw new InvalidOperationException("Unknown persistence type.")
            };

            model.Id = id;
            transactionScope.Complete();

            return model.Id;
        }

        public async Task<T> ReadAsync(long id)
        {
            T ret;
            using TransactionScope transactionScope = new TransactionScope();
            using IDbConnection connection = _provideConnection.Connection;
            ret = true switch
            {
                true when typeof(T) == typeof(IAccountSql) => await connection.GetAsync<Account>(id)
                    .ConfigureAwait(false) is T t0
                    ? t0
                    : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(IBudgetEntrySql) => await connection.GetAsync<BudgetEntry>(id)
                    .ConfigureAwait(false) is T t1
                    ? t1
                    : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(ICategorySql) => await connection.GetAsync<Category>(id)
                    .ConfigureAwait(false) is T t2
                    ? t2
                    : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(IDbSettingSql) => await connection.GetAsync<DbSetting>(id)
                    .ConfigureAwait(false) is T t3
                    ? t3
                    : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(IFlagSql) =>
                    await connection.GetAsync<Flag>(id).ConfigureAwait(false) is T t4
                        ? t4
                        : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(IPayeeSql) =>
                    await connection.GetAsync<Payee>(id).ConfigureAwait(false) is T t5
                        ? t5
                        : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(ISubTransactionSql) => await connection.GetAsync<SubTransaction>(id)
                    .ConfigureAwait(false) is T t6
                    ? t6
                    : throw new Exception("Shouldn't be null"),
                true when typeof(T) == typeof(ITransSql) =>
                    await connection.GetAsync<Trans>(id).ConfigureAwait(false) is T t7
                        ? t7
                        : throw new Exception("Shouldn't be null"),
                _ => throw new InvalidOperationException("Unknown persistence type.")
            };

            transactionScope.Complete();
            return ret;
        }

        public async Task<IEnumerable<T>> ReadAllAsync()
        {
            IList<T> ret;
            using TransactionScope transactionScope = new TransactionScope();
            using IDbConnection connection = _provideConnection.Connection;
            ret = true switch
            {
                true when typeof(T) == typeof(IAccountSql) => (await connection.GetAllAsync<Account>()
                        .ConfigureAwait(false)).Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(IBudgetEntrySql) => (await connection.GetAllAsync<BudgetEntry>()
                        .ConfigureAwait(false)).Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(ICategorySql) => (await connection.GetAllAsync<Category>()
                        .ConfigureAwait(false)).Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(IDbSettingSql) => (await connection.GetAllAsync<DbSetting>()
                        .ConfigureAwait(false)).Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(IFlagSql) => (await connection.GetAllAsync<Flag>().ConfigureAwait(false))
                    .Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(IPayeeSql) =>
                    (await connection.GetAllAsync<Payee>().ConfigureAwait(false)).Cast<T>().ToList(),
                true when typeof(T) == typeof(ISubTransactionSql) => (await connection.GetAllAsync<SubTransaction>()
                        .ConfigureAwait(false)).Cast<T>()
                    .ToList(),
                true when typeof(T) == typeof(ITransSql) =>
                    (await connection.GetAllAsync<Trans>().ConfigureAwait(false)).Cast<T>().ToList(),
                _ => throw new InvalidOperationException("Unknown persistence type.")
            };

            transactionScope.Complete();

            return ret;
        }

        public async Task UpdateAsync(T model)
        {
            if (model.Id <= 0) return;
            using TransactionScope transactionScope = new TransactionScope();
            using IDbConnection connection = _provideConnection.Connection;
            switch (model)
            {
                case IAccountSql account:
                    await connection.UpdateAsync(account as Account).ConfigureAwait(false);
                    break;
                case IBudgetEntrySql budgetEntry:
                    await connection.UpdateAsync(budgetEntry as BudgetEntry).ConfigureAwait(false);
                    break;
                case ICategorySql category:
                    await connection.UpdateAsync(category as Category).ConfigureAwait(false);
                    break;
                case IDbSettingSql dbSetting:
                    await connection.UpdateAsync(dbSetting as DbSetting).ConfigureAwait(false);
                    break;
                case IFlagSql flag:
                    await connection.UpdateAsync(flag as Flag).ConfigureAwait(false);
                    break;
                case IPayeeSql payee:
                    await connection.UpdateAsync(payee as Payee).ConfigureAwait(false);
                    break;
                case ISubTransactionSql subTransaction:
                    await connection.UpdateAsync(subTransaction as SubTransaction).ConfigureAwait(false);
                    break;
                case ITransSql trans:
                    await connection.UpdateAsync(trans as Trans).ConfigureAwait(false);
                    break;
                default: throw new InvalidOperationException("Unknown persistence type.");
            }

            transactionScope.Complete();
        }

        public async Task DeleteAsync(T model)
        {
            if (model.Id <= 0) return;
            using TransactionScope transactionScope = new TransactionScope();
            using IDbConnection connection = _provideConnection.Connection;
            switch (model)
            {
                case IAccountSql account:
                    _provideConnection.Backup($"BeforeDeletionOfAccount{account.Name}");
                    connection.Execute(OnAccountDeletion, new { accountId = account.Id });
                    break;
                case ICategorySql category:
                    _provideConnection.Backup($"BeforeDeletionOfCategory{category.Name}");
                    connection.Execute(OnCategoryDeletion, new { categoryId = category.Id });
                    break;
                case IPayeeSql payee:
                    _provideConnection.Backup($"BeforeDeletionOfPayee{payee.Name}");
                    connection.Execute(OnPayeeDeletion, new { payeeId = payee.Id });
                    break;
                case IFlagSql flag:
                    _provideConnection.Backup($"BeforeDeletionOfFlag{flag.Name}");
                    break;
            }

            switch (model)
            {
                case IAccountSql account:
                    await connection.DeleteAsync(account as Account).ConfigureAwait(false);
                    break;
                case IBudgetEntrySql budgetEntry:
                    await connection.DeleteAsync(budgetEntry as BudgetEntry).ConfigureAwait(false);
                    break;
                case ICategorySql category:
                    await connection.DeleteAsync(category as Category).ConfigureAwait(false);
                    break;
                case IDbSettingSql dbSetting:
                    await connection.DeleteAsync(dbSetting as DbSetting).ConfigureAwait(false);
                    break;
                case IFlagSql flag:
                    await connection.DeleteAsync(flag as Flag).ConfigureAwait(false);
                    break;
                case IPayeeSql payee:
                    await connection.DeleteAsync(payee as Payee).ConfigureAwait(false);
                    break;
                case ISubTransactionSql subTransaction:
                    await connection.DeleteAsync(subTransaction as SubTransaction).ConfigureAwait(false);
                    break;
                case ITransSql trans:
                    await connection.DeleteAsync(trans as Trans).ConfigureAwait(false);
                    break;
                default: throw new InvalidOperationException("Unknown persistence type.");
            }
                
            transactionScope.Complete();
        }
    }
}
