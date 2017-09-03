using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateSubIncomeTable : CreateTableBase
    {
        public CreateSubIncomeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(SubIncome)}s](
            {nameof(SubIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(SubIncome.ParentId)} INTEGER,
            {nameof(SubIncome.CategoryId)} INTEGER,
            {nameof(SubIncome.Memo)} TEXT,
            {nameof(SubIncome.Sum)} INTEGER,
            FOREIGN KEY({nameof(SubIncome.ParentId)}) REFERENCES {nameof(ParentIncome)}s({nameof(ParentIncome.Id)}) ON DELETE CASCADE);";

    }

    public interface ISubIncomeRepository : ISubTransIncRepository<Domain.ISubIncome>
    {
    }

    public class SubIncomeRepository : SubTransIncRepository<Domain.ISubIncome, SubIncome>, ISubIncomeRepository
    {
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;

        public SubIncomeRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.ISubIncome Create() =>
            new Domain.SubIncome(this, -1L, null, "", 0L);
        
        protected override Converter<Domain.ISubIncome, SubIncome> ConvertToPersistence => domainSubIncome => 
            new SubIncome
            {
                Id = domainSubIncome.Id,
                ParentId = domainSubIncome.Parent.Id,
                CategoryId = domainSubIncome.Category.Id,
                Memo = domainSubIncome.Memo,
                Sum = domainSubIncome.Sum
            };

        protected override Converter<(SubIncome, DbConnection), Domain.ISubIncome> ConvertToDomain => tuple =>
        {
            (SubIncome persistenceSubIncome, DbConnection connection) = tuple;
            return new Domain.SubIncome(
                this,
                persistenceSubIncome.Id,
                _categoryFetcher(persistenceSubIncome.CategoryId, connection),
                persistenceSubIncome.Memo,
                persistenceSubIncome.Sum);
        };
    }
}