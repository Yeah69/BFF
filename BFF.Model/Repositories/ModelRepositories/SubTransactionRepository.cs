using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ISubTransactionRepository
    {
        Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId);
    }

    internal sealed class SubTransactionRepository : RepositoryBase<ISubTransaction, ISubTransactionSql>, ISubTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IParentalOrm _parentalOrm;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;

        public SubTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ISubTransactionSql> crudOrm,
            IParentalOrm parentalOrm,
            ICategoryBaseRepositoryInternal categoryBaseRepository)
            : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _parentalOrm = parentalOrm;
            _categoryBaseRepository = categoryBaseRepository;
        }

        public async Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId) =>
            await (await _parentalOrm.ReadSubTransactionsOfAsync(parentId).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override async Task<ISubTransaction> ConvertToDomainAsync(ISubTransactionSql persistenceModel)
        {
            return new SubTransaction<ISubTransactionSql>(
                persistenceModel,
                this,
                _rxSchedulerProvider,
                persistenceModel.Id > 0,
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum);
        }
    }
}