using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Helper;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.ORM
{
    internal class DapperMergeOrm : IMergeOrm
    {
        private static readonly string OnPayeeMerge = $@"
UPDATE {nameof(Trans)}s SET {nameof(Trans.PayeeId)} = @toPayeeId 
WHERE {nameof(Trans.Type)} = '{TransType.Transaction}' AND {nameof(Trans.PayeeId)} = @fromPayeeId;
UPDATE {nameof(Trans)}s SET {nameof(Trans.PayeeId)} = @toPayeeId 
WHERE {nameof(Trans.Type)} = '{TransType.ParentTransaction}' AND {nameof(Trans.PayeeId)} = @fromPayeeId;";

        private static readonly string OnFlagMerge = $@"
UPDATE {nameof(Trans)}s SET {nameof(Trans.FlagId)} = @toFlagId 
WHERE {nameof(Trans.FlagId)} = @fromFlagId;";

        private static readonly string OnCategoryMerge = $@"
UPDATE {nameof(Trans)}s SET {nameof(Trans.CategoryId)} = @toCategoryId 
WHERE {nameof(Trans.Type)} = '{TransType.Transaction}' AND {nameof(Trans.CategoryId)} = @fromCategoryId;
UPDATE {nameof(SubTransaction)}s SET {nameof(SubTransaction.CategoryId)} = @toCategoryId 
WHERE {nameof(SubTransaction.CategoryId)} = @fromCategoryId;
UPDATE {nameof(Category)}s SET {nameof(Category.ParentId)} = @toCategoryId 
WHERE {nameof(Category.ParentId)} = @fromCategoryId;";

        private readonly IProvideSqliteConnection _provideConnection;
        private readonly IPayeeRepositoryInternal _payeeRepository;

        public DapperMergeOrm(
            IProvideSqliteConnection provideConnection,
            IPayeeRepositoryInternal payeeRepository)
        {
            _provideConnection = provideConnection;
            _payeeRepository = payeeRepository;
        }

        public async Task MergePayeeAsync(IPayeeSql from, IPayeeSql to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                _provideConnection.Backup($"BeforeMergeOfPayee{from.Name}ToPayee{to.Name}");
                await connection.ExecuteAsync(OnPayeeMerge, new { fromPayeeId = from.Id, toPayeeId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task MergeFlagAsync(IFlagSql from, IFlagSql to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                _provideConnection.Backup($"BeforeMergeOfFlag{from.Name}ToFlag{to.Name}");
                await connection.ExecuteAsync(OnFlagMerge, new { fromFlagId = from.Id, toFlagId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task MergeCategoryAsync(ICategorySql from, ICategorySql to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                _provideConnection.Backup($"BeforeMergeOfCategory{from.Name}ToCategory{to.Name}");
                await connection.ExecuteAsync(OnCategoryMerge, new { fromCategoryId = from.Id, toCategoryId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }
    }
}
