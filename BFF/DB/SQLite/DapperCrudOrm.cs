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
                connection.Insert(models);
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
                connection.Delete(model);
                transactionScope.Complete();
            }
        }

        public void Delete<T>(IEnumerable<T> models) where T : class, IPersistenceModel
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                foreach (var model in models)
                {
                    connection.Delete(model);
                }
                transactionScope.Complete();
            }
        }
    }
}
