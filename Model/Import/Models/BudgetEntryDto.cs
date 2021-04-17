namespace BFF.Model.Import.Models
{
    public class BudgetEntryDto
    {
        public int MonthIndex { get; set; }

        public CategoryDto? Category { get; set; }

        public long Budget { get; set; }
    }
}
