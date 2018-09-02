using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class DapperCrudOrm : ICrudOrm
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

        private readonly IProvideConnection _provideConnection;

        public DapperCrudOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task CreateAsync<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                long id = await connection.InsertAsync(model).ConfigureAwait(false);
                model.Id = id;
                transactionScope.Complete();
            }
        }

        public async Task CreateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                foreach (var model in models)
                {
                    long id = await connection.InsertAsync(model).ConfigureAwait(false);
                    model.Id = id;
                }
                transactionScope.Complete();
            }
        }

        public async Task<T> ReadAsync<T>(long id) where T : class, IPersistenceModel
        {
            T ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = await connection.GetAsync<T>(id).ConfigureAwait(false);
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<T>> ReadAllAsync<T>() where T : class, IPersistenceModel
        {
            IList<T> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = (await connection.GetAllAsync<T>().ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task UpdateAsync<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                await connection.UpdateAsync(model).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task UpdateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                foreach (var model in models)
                {
                    await connection.UpdateAsync(model).ConfigureAwait(false);
                }
                transactionScope.Complete();
            }
        }

        public async Task DeleteAsync<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                switch (model)
                {
                    case Account account:
                        _provideConnection.Backup($"BeforeDeletionOfAccount{account.Name}");
                        connection.Execute(OnAccountDeletion, new { accountId = account.Id });
                        break;
                    case Category category:
                        _provideConnection.Backup($"BeforeDeletionOfCategory{category.Name}");
                        connection.Execute(OnCategoryDeletion, new { categoryId = category.Id });
                        break;
                    case Payee payee:
                        _provideConnection.Backup($"BeforeDeletionOfPayee{payee.Name}");
                        connection.Execute(OnPayeeDeletion, new { payeeId = payee.Id });
                        break;
                    case Flag flag:
                        _provideConnection.Backup($"BeforeDeletionOfFlag{flag.Name}");
                        break;
                }

                await connection.DeleteAsync(model).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task DeleteAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            foreach (var model in models)
            {
                await DeleteAsync(model).ConfigureAwait(false);
            }
        }
    }
}
