using System;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IBudgetEntryDto : IPersistenceModelDto, IHaveCategoryDto
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
    }
    
    internal class BudgetEntry : IBudgetEntryDto
    {
        [Key]
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}