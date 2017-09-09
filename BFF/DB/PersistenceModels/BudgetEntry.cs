using System;

namespace BFF.DB.PersistenceModels
{
    public class BudgetEntry : IPersistenceModel
    {
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}