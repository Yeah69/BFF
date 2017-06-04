using System;

namespace BFF.DB.PersistanceModels
{
    public class BudgetEntry : IPersistanceModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}