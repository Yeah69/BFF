using System;
using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class Account : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTime StartingDate { get; set; }
    }
}