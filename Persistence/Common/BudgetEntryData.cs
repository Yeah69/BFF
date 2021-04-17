using System;

namespace BFF.Persistence.Common
{
    internal struct BudgetEntryData
    {
        public DateTime Month { get; set; }
        
        public long Budget { get; set; }
        
        public long Outflow { get; set; }
        
        public long Balance { get; set; }
        
        public long AggregatedBudget { get; set; }
        
        public long AggregatedOutflow { get; set; }
        
        public long AggregatedBalance { get; set; }
    }
}