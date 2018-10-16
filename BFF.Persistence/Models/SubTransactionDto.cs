using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class SubTransactionDto : IPersistenceModelDto, IHaveCategoryDto
    {
        [Key]
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long? CategoryId { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
    }
}