using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperCrudOrm : ICrudOrm
    {
        private static readonly string OnAccountDeletion = $@"
DELETE FROM {nameof(TransDto)}s WHERE {nameof(TransDto.AccountId)} = @accountId AND ({nameof(TransDto.Type)} = '{TransType.Transaction}' OR {nameof(TransDto.Type)} = '{TransType.ParentTransaction}');
DELETE FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND ({nameof(TransDto.PayeeId)} IS NULL AND {nameof(TransDto.CategoryId)} = @accountId OR {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.CategoryId)} IS NULL);
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.PayeeId)} = NULL WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.CategoryId)} IS NOT NULL;
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.CategoryId)} = NULL WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.CategoryId)} = @accountId AND {nameof(TransDto.PayeeId)} IS NOT NULL;";
        
        private static readonly string OnCategoryDeletion = $@"
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.CategoryId)} = NULL WHERE {nameof(TransDto.Type)} = '{TransType.Transaction}' AND {nameof(TransDto.CategoryId)} = @categoryId;
UPDATE {nameof(SubTransactionDto)}s SET {nameof(SubTransactionDto.CategoryId)} = NULL WHERE {nameof(SubTransactionDto.CategoryId)} = @categoryId;";

        private static readonly string OnPayeeDeletion = $@"
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.PayeeId)} = NULL WHERE {nameof(TransDto.Type)} = '{TransType.Transaction}' AND {nameof(TransDto.PayeeId)} = @payeeId;
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.PayeeId)} = NULL WHERE {nameof(TransDto.Type)} = '{TransType.ParentTransaction}' AND {nameof(TransDto.PayeeId)} = @payeeId;";

        private readonly IProvideConnection _provideConnection;

        public DapperCrudOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task CreateAsync<T>(T model) where T : class, IPersistenceModelDto
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                long id = await connection.InsertAsync(model).ConfigureAwait(false);
                model.Id = id;
                transactionScope.Complete();
            }
        }

        public async Task CreateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                foreach (var model in models)
                {
                    long id = await connection.InsertAsync(model).ConfigureAwait(false);
                    model.Id = id;
                }
                transactionScope.Complete();
            }
        }

        public async Task<T> ReadAsync<T>(long id) where T : class, IPersistenceModelDto
        {
            T ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.GetAsync<T>(id).ConfigureAwait(false);
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<T>> ReadAllAsync<T>() where T : class, IPersistenceModelDto
        {
            IList<T> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.GetAllAsync<T>().ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task UpdateAsync<T>(T model) where T : class, IPersistenceModelDto
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                await connection.UpdateAsync(model).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task UpdateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                foreach (var model in models)
                {
                    await connection.UpdateAsync(model).ConfigureAwait(false);
                }
                transactionScope.Complete();
            }
        }

        public async Task DeleteAsync<T>(T model) where T : class, IPersistenceModelDto
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                switch (model)
                {
                    case AccountDto account:
                        _provideConnection.Backup($"BeforeDeletionOfAccount{account.Name}");
                        connection.Execute(OnAccountDeletion, new { accountId = account.Id });
                        break;
                    case CategoryDto category:
                        _provideConnection.Backup($"BeforeDeletionOfCategory{category.Name}");
                        connection.Execute(OnCategoryDeletion, new { categoryId = category.Id });
                        break;
                    case PayeeDto payee:
                        _provideConnection.Backup($"BeforeDeletionOfPayee{payee.Name}");
                        connection.Execute(OnPayeeDeletion, new { payeeId = payee.Id });
                        break;
                    case FlagDto flag:
                        _provideConnection.Backup($"BeforeDeletionOfFlag{flag.Name}");
                        break;
                }

                await connection.DeleteAsync(model).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task DeleteAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto
        {
            foreach (var model in models)
            {
                await DeleteAsync(model).ConfigureAwait(false);
            }
        }
    }
}
