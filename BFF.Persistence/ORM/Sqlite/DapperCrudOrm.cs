using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Helper;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
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

        public async Task<bool> CreateAsync(T model)
        {
            if (model.Id > 0) return true;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                long id;

                switch (model)
                {
                    case IAccountSql account:
                        id = await connection.InsertAsync(account as Account).ConfigureAwait(false);
                        break;
                    case IBudgetEntrySql budgetEntry:
                        id = await connection.InsertAsync(budgetEntry as BudgetEntry).ConfigureAwait(false);
                        break;
                    case ICategorySql category:
                        id = await connection.InsertAsync(category as Category).ConfigureAwait(false);
                        break;
                    case IDbSettingSql dbSetting:
                        id = await connection.InsertAsync(dbSetting as DbSetting).ConfigureAwait(false);
                        break;
                    case IFlagSql flag:
                        id = await connection.InsertAsync(flag as Flag).ConfigureAwait(false);
                        break;
                    case IPayeeSql payee:
                        id = await connection.InsertAsync(payee as Payee).ConfigureAwait(false);
                        break;
                    case ISubTransactionSql subTransaction:
                        id = await connection.InsertAsync(subTransaction as SubTransaction).ConfigureAwait(false);
                        break;
                    case ITransSql trans:
                        id = await connection.InsertAsync(trans as Trans).ConfigureAwait(false);
                        break;
                    default: throw new InvalidOperationException("Unknown persistence type.");
                }

                model.Id = id;
                transactionScope.Complete();
            }

            return true;
        }

        public async Task<T> ReadAsync(long id)
        {
            T ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                switch (true)
                {
                    case true when typeof(T) == typeof(IAccountSql):
                        ret = await connection.GetAsync<Account>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(IBudgetEntrySql):
                        ret = await connection.GetAsync<BudgetEntry>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(ICategorySql):
                        ret = await connection.GetAsync<Category>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(IDbSettingSql):
                        ret = await connection.GetAsync<DbSetting>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(IFlagSql):
                        ret = await connection.GetAsync<Flag>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(IPayeeSql):
                        ret = await connection.GetAsync<Payee>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(ISubTransactionSql):
                        ret = await connection.GetAsync<SubTransaction>(id).ConfigureAwait(false) as T;
                        break;
                    case true when typeof(T) == typeof(ITransSql):
                        ret = await connection.GetAsync<Trans>(id).ConfigureAwait(false) as T;
                        break;
                    default: throw new InvalidOperationException("Unknown persistence type.");
                }

                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<T>> ReadAllAsync()
        {
            IList<T> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                switch (true)
                {
                    case true when typeof(T) == typeof(IAccountSql):
                        ret = (await connection.GetAllAsync<Account>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(IBudgetEntrySql):
                        ret = (await connection.GetAllAsync<BudgetEntry>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(ICategorySql):
                        ret = (await connection.GetAllAsync<Category>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(IDbSettingSql):
                        ret = (await connection.GetAllAsync<DbSetting>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(IFlagSql):
                        ret = (await connection.GetAllAsync<Flag>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(IPayeeSql):
                        ret = (await connection.GetAllAsync<Payee>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(ISubTransactionSql):
                        ret = (await connection.GetAllAsync<SubTransaction>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    case true when typeof(T) == typeof(ITransSql):
                        ret = (await connection.GetAllAsync<Trans>().ConfigureAwait(false)).Cast<T>().ToList();
                        break;
                    default: throw new InvalidOperationException("Unknown persistence type.");
                }

                transactionScope.Complete();
            }

            return ret;
        }

        public async Task UpdateAsync(T model)
        {
            if (model.Id <= 0) return;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
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
        }

        public async Task DeleteAsync(T model)
        {
            if (model.Id <= 0) return;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
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
}
