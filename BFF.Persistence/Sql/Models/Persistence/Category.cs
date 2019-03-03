using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface ICategorySql : IPersistenceModelSql
    {
        long? ParentId { get; set; }
        string Name { get; set; }
        bool IsIncomeRelevant { get; set; }
        int MonthOffset { get; set; }
    }
    
    internal class Category : ICategorySql
    {
        [Key]
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }
    }
}