using System;

namespace BFF.DB.PersistenceModels
{
    public class Transfer : IPersistenceModel
    {
        public long Id { get; set; }
        public string CheckNumber { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
        public long Cleared { get; set; }
    }
}