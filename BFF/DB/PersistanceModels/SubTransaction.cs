using System;

namespace BFF.DB.PersistanceModels
{
    public class SubTransaction : IPersistanceModel
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long CategoryId { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
    }
}