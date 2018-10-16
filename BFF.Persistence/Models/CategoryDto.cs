using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class CategoryDto : IPersistenceModelDto
    {
        [Key]
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }
    }
}