using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface ISqliteSubTransactionRepository
    {
        Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId);
    }

    internal sealed class SqliteSubTransactionRepository : SqliteRepositoryBase<ISubTransaction, ISubTransactionSql>, ISqliteSubTransactionRepository
    {
        private readonly ICrudOrm<ISubTransactionSql> _crudOrm;
        private readonly Lazy<IParentalOrm> _parentalOrm;
        private readonly Lazy<ISqliteCategoryBaseRepositoryInternal> _categoryBaseRepository;

        public SqliteSubTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ISubTransactionSql> crudOrm,
            Lazy<IParentalOrm> parentalOrm,
            Lazy<ISqliteCategoryBaseRepositoryInternal> categoryBaseRepository)
            : base(crudOrm)
        {
            _crudOrm = crudOrm;
            _parentalOrm = parentalOrm;
            _categoryBaseRepository = categoryBaseRepository;
        }

        public async Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId) =>
            await (await _parentalOrm.Value.ReadSubTransactionsOfAsync(parentId).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override async Task<ISubTransaction> ConvertToDomainAsync(ISubTransactionSql persistenceModel)
        {
            return new Models.Domain.SubTransaction(
                _crudOrm,
                persistenceModel.Id,
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.Value.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum);
        }
    }
}