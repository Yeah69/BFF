namespace BFF.Persistence.Import.Models
{
    internal class SubTransactionDto
    {
        public CategoryDto Category { get; set; }

        public string Memo { get; set; }

        public long Sum { get; set; }
    }
}
