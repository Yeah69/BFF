using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    public class DapperMergeOrm : IMergeOrm
    {
        private static readonly string OnPayeeMerge = $@"
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.PayeeId)} = @toPayeeId 
WHERE {nameof(TransDto.Type)} = '{TransType.Transaction}' AND {nameof(TransDto.PayeeId)} = @fromPayeeId;
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.PayeeId)} = @toPayeeId 
WHERE {nameof(TransDto.Type)} = '{TransType.ParentTransaction}' AND {nameof(TransDto.PayeeId)} = @fromPayeeId;";

        private static readonly string OnFlagMerge = $@"
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.FlagId)} = @toFlagId 
WHERE {nameof(TransDto.FlagId)} = @fromFlagId;";

        private static readonly string OnCategoryMerge = $@"
UPDATE {nameof(TransDto)}s SET {nameof(TransDto.CategoryId)} = @toCategoryId 
WHERE {nameof(TransDto.Type)} = '{TransType.Transaction}' AND {nameof(TransDto.CategoryId)} = @fromCategoryId;
UPDATE {nameof(SubTransactionDto)}s SET {nameof(SubTransactionDto.CategoryId)} = @toCategoryId 
WHERE {nameof(SubTransactionDto.CategoryId)} = @fromCategoryId;
UPDATE {nameof(CategoryDto)}s SET {nameof(CategoryDto.ParentId)} = @toCategoryId 
WHERE {nameof(CategoryDto.ParentId)} = @fromCategoryId;";

        private readonly IProvideConnection _provideConnection;

        public DapperMergeOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task MergePayeeAsync(PayeeDto from, PayeeDto to)
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

        public async Task MergeFlagAsync(FlagDto from, FlagDto to)
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

        public async Task MergeCategoryAsync(CategoryDto from, CategoryDto to)
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
