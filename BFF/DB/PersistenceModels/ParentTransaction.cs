using System;

namespace BFF.DB.PersistenceModels
{
    public class ParentTransaction : IPersistenceModel, IHaveAccount, IHavePayee
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long PayeeId { get; set; }
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long Cleared { get; set; }
    }
}