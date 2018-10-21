using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface ISubTransactionDto : IPersistenceModelDto, IHaveCategoryDto
    {
        long ParentId { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
    
    internal class SubTransaction : ISubTransactionDto
    {
        [Key]
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long? CategoryId { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
    }
}