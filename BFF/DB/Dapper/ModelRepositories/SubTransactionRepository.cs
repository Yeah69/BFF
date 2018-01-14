using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using Dapper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateSubTransactionTable : CreateTableBase
    {
        public CreateSubTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(SubTransaction)}s](
            {nameof(SubTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(SubTransaction.ParentId)} INTEGER,
            {nameof(SubTransaction.CategoryId)} INTEGER,
            {nameof(SubTransaction.Memo)} TEXT,
            {nameof(SubTransaction.Sum)} INTEGER,
            FOREIGN KEY({nameof(SubTransaction.ParentId)}) REFERENCES {nameof(Trans)}s({nameof(Trans.Id)}) ON DELETE CASCADE);";

    }

    public interface ISubTransactionRepository : IRepositoryBase<Domain.ISubTransaction>
    {
        IEnumerable<Domain.ISubTransaction> GetChildrenOf(long parentId, DbConnection connection = null);
    }

    public sealed class SubTransactionRepository : RepositoryBase<Domain.ISubTransaction, SubTransaction>, ISubTransactionRepository
    {
        private readonly ICategoryBaseRepository _categoryBaseRepository;

        public SubTransactionRepository(
            IProvideConnection provideConnection,
            ICategoryBaseRepository categoryBaseRepository) : base(provideConnection)
        {
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

        protected override Converter<(SubTransaction, DbConnection), Domain.ISubTransaction> ConvertToDomain => tuple =>
        {
            (SubTransaction persistenceSubTransaction, DbConnection connection) = tuple;
            return new Domain.SubTransaction(
                this,
                persistenceSubTransaction.Id,
                persistenceSubTransaction.CategoryId == null ? null : _categoryBaseRepository.Find((long)persistenceSubTransaction.CategoryId, connection),
                persistenceSubTransaction.Memo,
                persistenceSubTransaction.Sum);
        };

        private string ParentalQuery => 
            $"SELECT * FROM [{typeof(SubTransaction).Name}s] WHERE {nameof(SubTransaction.ParentId)} = @ParentId;";

        public IEnumerable<Domain.ISubTransaction> GetChildrenOf(long parentId, DbConnection connection = null) => 
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<SubTransaction>(ParentalQuery, new {ParentId = parentId}).Select(sti => ConvertToDomain( (sti, c) )),
                ProvideConnection,
                connection);
    }
}