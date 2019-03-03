using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface ITransSql : IPersistenceModelSql, IHaveAccountSql, IHaveCategorySql, IHavePayeeSql, IHaveFlagSql
    {
        string CheckNumber { get; set; }
        DateTime Date { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
        long Cleared { get; set; }
        string Type { get; set; }
    }
    
    internal class Trans : ITransSql
    {
        [Key]
        public long Id { get; set; }
        public long? FlagId { get; set; }
        public string CheckNumber { get; set; }
        public long AccountId { get; set; }
        public long? PayeeId { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
        public long Cleared { get; set; }
        public string Type { get; set; }
    }
}