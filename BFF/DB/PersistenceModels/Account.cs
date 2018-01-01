using System;

namespace BFF.DB.PersistenceModels
{
    public class Account : IPersistenceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTime StartingDate { get; set; }
    }
}