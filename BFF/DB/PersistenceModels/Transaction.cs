using System;

namespace BFF.DB.PersistenceModels
{
    public class Transaction : IPersistenceModel, IHaveAccount, IHavePayee, IHaveCategory
    {
        public long Id { get; set; }
        public string CheckNumber { get; set; }
        public long AccountId { get; set; }
        public long PayeeId { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
        public long Cleared { get; set; }
    }
}