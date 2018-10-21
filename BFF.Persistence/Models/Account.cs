using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IAccountDto : IPersistenceModelDto
    {
        string Name { get; set; }
        long StartingBalance { get; set; }
        DateTime StartingDate { get; set; }
    }
    
    internal class Account : IAccountDto
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTime StartingDate { get; set; }
    }
}