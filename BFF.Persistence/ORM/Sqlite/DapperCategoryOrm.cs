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
        private static readonly string GetAllCategoriesQuery = $"SELECT * FROM {nameof(CategoryDto)}s WHERE {nameof(CategoryDto.IsIncomeRelevant)} == 0;";
        private static readonly string GetAllIncomeCategoriesQuery = $"SELECT * FROM {nameof(CategoryDto)}s WHERE {nameof(CategoryDto.IsIncomeRelevant)} == 1;";

        private readonly IProvideConnection _provideConnection;

        public DapperCategoryOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<IEnumerable<CategoryDto>> ReadCategoriesAsync()
        {
            IList<CategoryDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<CategoryDto>(GetAllCategoriesQuery).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<CategoryDto>> ReadIncomeCategoriesAsync()
        {
            IList<CategoryDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<CategoryDto>(GetAllIncomeCategoriesQuery).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
