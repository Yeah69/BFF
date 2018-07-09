using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;
using Category = BFF.DB.PersistenceModels.Category;
using Flag = BFF.DB.PersistenceModels.Flag;
using Payee = BFF.DB.PersistenceModels.Payee;
using SubTransaction = BFF.DB.PersistenceModels.SubTransaction;

namespace BFF.DB.SQLite
{
    public class DapperMergeOrm : IMergeOrm
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

        private readonly IProvideConnection _provideConnection;

        public DapperMergeOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task MergePayeeAsync(Payee from, Payee to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                
                _provideConnection.Backup($"BeforeMergeOfPayee{from.Name}ToPayee{to.Name}");
                await connection.ExecuteAsync(OnPayeeMerge, new { fromPayeeId = from.Id, toPayeeId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task MergeFlagAsync(Flag from, Flag to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                _provideConnection.Backup($"BeforeMergeOfFlag{from.Name}ToFlag{to.Name}");
                await connection.ExecuteAsync(OnFlagMerge, new { fromFlagId = from.Id, toFlagId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }

        public async Task MergeCategoryAsync(Category from, Category to)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                _provideConnection.Backup($"BeforeMergeOfCategory{from.Name}ToCategory{to.Name}");
                await connection.ExecuteAsync(OnCategoryMerge, new { fromCategoryId = from.Id, toCategoryId = to.Id }).ConfigureAwait(false);

                await connection.DeleteAsync(from).ConfigureAwait(false);
                transactionScope.Complete();
            }
        }
    }
}
