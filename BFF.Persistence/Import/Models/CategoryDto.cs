﻿namespace BFF.Persistence.Import.Models
{
    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public bool IsIncomeRelevant { get; set; }

        public int IncomeMonthOffset { get; set; }
    }
}
