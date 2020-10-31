namespace BFF.Persistence.Import.Models
{
    internal class BudgetEntryDto
    {
        public int MonthIndex { get; set; }

        public CategoryDto? Category { get; set; }

        public long Budget { get; set; }
    }
}
