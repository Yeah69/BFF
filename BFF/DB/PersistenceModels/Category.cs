using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class Category : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }
    }
}