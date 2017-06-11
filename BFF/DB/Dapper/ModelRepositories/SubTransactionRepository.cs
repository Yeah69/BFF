using System;
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
    
    public class SubTransactionRepository : SubTransIncRepository<Domain.SubTransaction, Persistance.SubTransaction>
    {
        public SubTransactionRepository(IProvideConnection provideConnection) : base(provideConnection) { }

        public override Domain.SubTransaction Create() =>
            new Domain.SubTransaction(this);
        
        protected override Converter<Domain.SubTransaction, Persistance.SubTransaction> ConvertToPersistance => domainSubTransaction => 
            new Persistance.SubTransaction
            {
                Id = domainSubTransaction.Id,
                ParentId = domainSubTransaction.ParentId,
                CategoryId = domainSubTransaction.CategoryId,
                Memo = domainSubTransaction.Memo,
                Sum = domainSubTransaction.Sum
            };
        
        protected override Converter<Persistance.SubTransaction, Domain.SubTransaction> ConvertToDomain => persistanceSubTransaction =>
            new Domain.SubTransaction(this)
            {
                Id = persistanceSubTransaction.Id,
                ParentId = persistanceSubTransaction.ParentId,
                CategoryId = persistanceSubTransaction.CategoryId,
                Memo = persistanceSubTransaction.Memo,
                Sum = persistanceSubTransaction.Sum
            };
    }
}