using System;
using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class Trans : IPersistenceModel, IHaveAccount, IHaveCategory, IHavePayee, IHaveFlag
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