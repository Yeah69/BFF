using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperCategoryOrm : ICategoryOrm
    {
        private static readonly string GetAllCategoriesQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 0;";
        private static readonly string GetAllIncomeCategoriesQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 1;";

        private readonly IProvideConnection _provideConnection;

        public DapperCategoryOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<IEnumerable<Category>> ReadCategoriesAsync()
        {
            IList<Category> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<Category>(GetAllCategoriesQuery).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Category>> ReadIncomeCategoriesAsync()
        {
            IList<Category> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<Category>(GetAllIncomeCategoriesQuery).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
