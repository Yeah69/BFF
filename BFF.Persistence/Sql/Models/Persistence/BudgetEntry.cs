using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IBudgetEntrySql : IPersistenceModelSql, IHaveCategorySql
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
    }
    
    internal class BudgetEntry : IBudgetEntrySql
    {
        [Key]
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}