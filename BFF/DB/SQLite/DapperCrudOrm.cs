using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistenceModels;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class DapperCrudOrm : ICrudOrm
    {
        private readonly IProvideConnection _provideConnection;

        public DapperCrudOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public void Create<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                long id = newConnection.Insert(model);
                model.Id = id;
                transactionScope.Complete();
            }
        }

        public void Create<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                newConnection.Insert(models);
                transactionScope.Complete();
            }
        }

        public T Read<T>(long id) where T : class, IPersistenceModel
        {
            T ret;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = newConnection.Get<T>(id);
                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> ReadAll<T>() where T : class, IPersistenceModel
        {
            IList<T> ret;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = newConnection.GetAll<T>().ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public void Update<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                newConnection.Update(model);
                transactionScope.Complete();
            }
        }

        public void Update<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                foreach (var model in models)
                {
                    newConnection.Update(model);
                }
                transactionScope.Complete();
            }
        }

        public void Delete<T>(T model) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                newConnection.Delete(model);
                transactionScope.Complete();
            }
        }

        public void Delete<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                foreach (var model in models)
                {
                    newConnection.Delete(model);
                }
                transactionScope.Complete();
            }
        }
    }
}
