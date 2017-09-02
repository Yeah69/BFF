using System;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateSubTransactionTable : CreateTableBase
    {
        public CreateSubTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.SubTransaction)}s](
            {nameof(Persistance.SubTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.SubTransaction.ParentId)} INTEGER,
            {nameof(Persistance.SubTransaction.CategoryId)} INTEGER,
            {nameof(Persistance.SubTransaction.Memo)} TEXT,
            {nameof(Persistance.SubTransaction.Sum)} INTEGER,
            FOREIGN KEY({nameof(Persistance.SubTransaction.ParentId)}) REFERENCES {nameof(Persistance.ParentTransaction)}s({nameof(Persistance.ParentTransaction.Id)}) ON DELETE CASCADE);";
        
    }
    
    public class SubTransactionRepository : SubTransIncRepository<Domain.ISubTransaction, Persistance.SubTransaction>
    {
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;

        public SubTransactionRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.ISubTransaction Create() =>
            new Domain.SubTransaction(this, -1L, null, "", 0L);
        
        protected override Converter<Domain.ISubTransaction, Persistance.SubTransaction> ConvertToPersistance => domainSubTransaction => 
            new Persistance.SubTransaction
            {
                Id = domainSubTransaction.Id,
                ParentId = domainSubTransaction.Parent.Id,
                CategoryId = domainSubTransaction.Category.Id,
                Memo = domainSubTransaction.Memo,
                Sum = domainSubTransaction.Sum
            };

        protected override Converter<(Persistance.SubTransaction, DbConnection), Domain.ISubTransaction> ConvertToDomain => tuple =>
        {
            (Persistance.SubTransaction persistenceSubTransaction, DbConnection connection) = tuple;
            return new Domain.SubTransaction(
                this,
                persistenceSubTransaction.Id,
                _categoryFetcher(persistenceSubTransaction.CategoryId, connection),
                persistenceSubTransaction.Memo,
                persistenceSubTransaction.Sum);
        };
    }
}