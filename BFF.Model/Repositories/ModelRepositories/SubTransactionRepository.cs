using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ISubTransactionRepository : IRepositoryBase<ISubTransaction>
    {
        Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId);
    }

    internal sealed class SubTransactionRepository : RepositoryBase<ISubTransaction, ISubTransactionSql>, ISubTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IParentalOrm _parentalOrm;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;
        private readonly Func<ISubTransactionSql> _subTransactionDtoFactory;

        public SubTransactionRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IParentalOrm parentalOrm,
            ICategoryBaseRepositoryInternal categoryBaseRepository,
            Func<ISubTransactionSql> subTransactionDtoFactory)
            : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _parentalOrm = parentalOrm;
            _categoryBaseRepository = categoryBaseRepository;
            _subTransactionDtoFactory = subTransactionDtoFactory;
        }
        
        protected override Converter<ISubTransaction, ISubTransactionSql> ConvertToPersistence => domainSubTransaction =>
        {
            var subTransactionDto = _subTransactionDtoFactory();
            subTransactionDto.Id = domainSubTransaction.Id;
            subTransactionDto.ParentId = domainSubTransaction.Parent.Id;
            subTransactionDto.CategoryId = domainSubTransaction.Category?.Id;
            subTransactionDto.Memo = domainSubTransaction.Memo;
            subTransactionDto.Sum = domainSubTransaction.Sum;
            return subTransactionDto;
        };

        public async Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId) =>
            await (await _parentalOrm.ReadSubTransactionsOfAsync(parentId).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override async Task<ISubTransaction> ConvertToDomainAsync(ISubTransactionSql persistenceModel)
        {
            return new SubTransaction(
                this,
                _rxSchedulerProvider,
                persistenceModel.Id,
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum);
        }
    }
}