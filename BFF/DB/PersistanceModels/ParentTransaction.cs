using System;

namespace BFF.DB.PersistanceModels
{
    public class ParentTransaction : IPersistanceModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long PayeeId { get; set; }
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long Cleared { get; set; }
    }
}