using System;

namespace BFF.Persistence.Common
{
    internal record BudgetEntryData(
        DateTime Month,
        long Budget,
        long Outflow,
        long Balance,
        long AggregatedBudget,
        long AggregatedOutflow,
        long AggregatedBalance);
}