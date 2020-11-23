using System;
using System.Drawing;

namespace BFF.Model.Import.Models
{
    public class TransactionDto
    {
        public DateTime Date { get; set; }

        public string Account { get; set; } = string.Empty;

        public string Payee { get; set; } = string.Empty;

        public CategoryDto? Category { get; set; }

        public string CheckNumber { get; set; } = string.Empty;

        public (string Name, Color Color)? Flag { get; set; }

        public string Memo { get; set; } = string.Empty;

        public long Sum { get; set; }

        public bool Cleared { get; set; }
    }
}
