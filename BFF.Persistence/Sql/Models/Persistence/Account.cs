using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IAccountSql : IPersistenceModelSql
    {
        string Name { get; set; }
        long StartingBalance { get; set; }
        DateTime StartingDate { get; set; }
    }
    
    internal class Account : IAccountSql
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public long StartingBalance { get; set; }
        public DateTime StartingDate { get; set; }
    }
}