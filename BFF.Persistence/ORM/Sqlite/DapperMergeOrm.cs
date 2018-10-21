﻿using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
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

        private readonly IProvideConnection _provideConnection;

        public DapperMergeOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task MergePayeeAsync(IPayeeDto from, IPayeeDto to)
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

        public async Task MergeFlagAsync(IFlagDto from, IFlagDto to)
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

        public async Task MergeCategoryAsync(ICategoryDto from, ICategoryDto to)
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
