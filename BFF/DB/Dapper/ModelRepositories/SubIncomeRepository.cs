using System;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateSubIncomeTable : CreateTableBase
    {
        public CreateSubIncomeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.SubIncome)}s](
            {nameof(Persistance.SubIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.SubIncome.ParentId)} INTEGER,
            {nameof(Persistance.SubIncome.CategoryId)} INTEGER,
            {nameof(Persistance.SubIncome.Memo)} TEXT,
            {nameof(Persistance.SubIncome.Sum)} INTEGER,
            FOREIGN KEY({nameof(Persistance.SubIncome.ParentId)}) REFERENCES {nameof(Persistance.ParentIncome)}s({nameof(Persistance.ParentIncome.Id)}) ON DELETE CASCADE);";
        
    }
    
    public class SubIncomeRepository : SubTransIncRepository<Domain.SubIncome, Persistance.SubIncome>
    {
        private readonly Func<long, DbConnection, Domain.IParentIncome> _parentIncomeFetcher;
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;

        public SubIncomeRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IParentIncome> parentIncomeFetcher,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _parentIncomeFetcher = parentIncomeFetcher;
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.SubIncome Create() =>
            new Domain.SubIncome(this, -1L, null, "", 0L);
        
        protected override Converter<Domain.SubIncome, Persistance.SubIncome> ConvertToPersistance => domainSubIncome => 
            new Persistance.SubIncome
            {
                Id = domainSubIncome.Id,
                ParentId = domainSubIncome.Parent.Id,
                CategoryId = domainSubIncome.Category.Id,
                Memo = domainSubIncome.Memo,
                Sum = domainSubIncome.Sum
            };

        protected override Converter<(Persistance.SubIncome, DbConnection), Domain.SubIncome> ConvertToDomain => tuple =>
        {
            (Persistance.SubIncome persistenceSubIncome, DbConnection connection) = tuple;
            return new Domain.SubIncome(
                this,
                persistenceSubIncome.Id,
                _categoryFetcher(persistenceSubIncome.CategoryId, connection),
                persistenceSubIncome.Memo,
                persistenceSubIncome.Sum);
        };
    }
}