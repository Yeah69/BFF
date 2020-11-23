namespace BFF.Model.Import.Models
{
    public class SubTransactionDto
    {
        public CategoryDto? Category { get; set; }

        public string Memo { get; set; } = string.Empty;

        public long Sum { get; set; }
    }
}
