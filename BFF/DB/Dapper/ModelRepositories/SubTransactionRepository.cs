using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Helper.Extensions;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ISubTransactionRepository : IRepositoryBase<Domain.ISubTransaction>
    {
        Task<IEnumerable<Domain.ISubTransaction>> GetChildrenOfAsync(long parentId);
    }

    public sealed class SubTransactionRepository : RepositoryBase<Domain.ISubTransaction, SubTransaction>, ISubTransactionRepository
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
        
        protected override Converter<Domain.ISubTransaction, SubTransaction> ConvertToPersistence => domainSubTransaction => 
            new SubTransaction
            {
                Id = domainSubTransaction.Id,
                ParentId = domainSubTransaction.Parent.Id,
                CategoryId = domainSubTransaction.Category?.Id,
                Memo = domainSubTransaction.Memo,
                Sum = domainSubTransaction.Sum
            };

        public async Task<IEnumerable<Domain.ISubTransaction>> GetChildrenOfAsync(long parentId) =>
            await (await _parentalOrm.ReadSubTransactionsOfAsync(parentId).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override async Task<Domain.ISubTransaction> ConvertToDomainAsync(SubTransaction persistenceModel)
        {
            return new Domain.SubTransaction(
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