using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ISubTransactionRepository : IRepositoryBase<Domain.ISubTransaction>
    {
        IEnumerable<Domain.ISubTransaction> GetChildrenOf(long parentId);
    }

    public sealed class SubTransactionRepository : RepositoryBase<Domain.ISubTransaction, SubTransaction>, ISubTransactionRepository
    {
        private readonly IParentalOrm _parentalOrm;
        private readonly ICategoryBaseRepository _categoryBaseRepository;

        public SubTransactionRepository(
            IProvideConnection provideConnection,
            ICrudOrm crudOrm,
            IParentalOrm parentalOrm,
            ICategoryBaseRepository categoryBaseRepository) : base(provideConnection, crudOrm)
        {
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

        protected override Converter<SubTransaction, Domain.ISubTransaction> ConvertToDomain => persistenceSubTransaction =>
            new Domain.SubTransaction(
                this,
                persistenceSubTransaction.Id,
                persistenceSubTransaction.CategoryId is null 
                    ? null 
                    : _categoryBaseRepository.Find((long)persistenceSubTransaction.CategoryId),
                persistenceSubTransaction.Memo,
                persistenceSubTransaction.Sum);

        public IEnumerable<Domain.ISubTransaction> GetChildrenOf(long parentId) =>
            _parentalOrm.ReadSubTransactionsOf(parentId).Select(sti => ConvertToDomain(sti));
    }
}