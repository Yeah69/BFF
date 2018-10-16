using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class BudgetEntryDto : IPersistenceModelDto, IHaveCategoryDto
    {
        [Key]
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}