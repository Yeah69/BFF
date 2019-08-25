using System;

namespace BFF.Persistence.Import.Models
{
    internal class BudgetEntryDto
    {
        public DateTime Month { get; set; }

        public CategoryDto Category { get; set; }

        public long Budget { get; set; }
    }
}
