﻿using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public void Create<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                long id = connection.Insert(model);
                model.Id = id;
                transactionScope.Complete();
            }
        }

        public void Create<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                foreach (var model in models)
                {
                    long id = connection.Insert(model);
                    model.Id = id;
                }
                transactionScope.Complete();
            }
        }

        public T Read<T>(long id) where T : class, IPersistenceModel
        {
            T ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Get<T>(id);
                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> ReadAll<T>() where T : class, IPersistenceModel
        {
            IList<T> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.GetAll<T>().ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public void Update<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                connection.Update(model);
                transactionScope.Complete();
            }
        }

        public void Update<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                foreach (var model in models)
                {
                    connection.Update(model);
                }
                transactionScope.Complete();
            }
        }

        public void Delete<T>(T model) where T : class, IPersistenceModel
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

                connection.Delete(model);
                transactionScope.Complete();
            }
        }

        public void Delete<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            foreach (var model in models)
            {
                this.Delete(model);
            }
        }
    }
}