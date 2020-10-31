using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface ISubTransactionSql : IPersistenceModelSql, IHaveCategorySql
    {
        long ParentId { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
    
    internal class SubTransaction : ISubTransactionSql
    {
        [Key]
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long? CategoryId { get; set; }
        public string Memo { get; set; } = String.Empty;
        public long Sum { get; set; }
    }
}