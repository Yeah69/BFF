using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ISubTransactionRepository : IRepositoryBase<ISubTransaction>
    {
        Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId);
    }

    public sealed class SubTransactionRepository : RepositoryBase<ISubTransaction, SubTransactionDto>, ISubTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IParentalOrm _parentalOrm;
        private readonly ICategoryBaseRepository _categoryBaseRepository;

        public SubTransactionRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IParentalOrm parentalOrm,
            ICategoryBaseRepository categoryBaseRepository) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _parentalOrm = parentalOrm;
            _categoryBaseRepository = categoryBaseRepository;
        }
        
        protected override Converter<ISubTransaction, SubTransactionDto> ConvertToPersistence => domainSubTransaction => 
            new SubTransactionDto
            {
                Id = domainSubTransaction.Id,
                ParentId = domainSubTransaction.Parent.Id,
                CategoryId = domainSubTransaction.Category?.Id,
                Memo = domainSubTransaction.Memo,
                Sum = domainSubTransaction.Sum
            };

        public async Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(long parentId) =>
            await (await _parentalOrm.ReadSubTransactionsOfAsync(parentId).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override async Task<ISubTransaction> ConvertToDomainAsync(SubTransactionDto persistenceModel)
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