using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistenceModels;
using Dapper;

namespace BFF.DB.SQLite
{
    class DapperCategoryOrm : ICategoryOrm
    {
        private static readonly string GetAllCategoriesQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 0;";
        private static readonly string GetAllIncomeCategoriesQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 1;";

        private readonly IProvideConnection _provideConnection;

        public DapperCategoryOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public IEnumerable<Category> ReadCategories()
        {
            IList<Category> ret;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = newConnection.Query<Category>(GetAllCategoriesQuery).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<Category> ReadIncomeCategories()
        {
            IList<Category> ret;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = newConnection.Query<Category>(GetAllIncomeCategoriesQuery).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
