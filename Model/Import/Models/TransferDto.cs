using System;
using System.Drawing;

namespace BFF.Model.Import.Models
{
    public class TransferDto
    {
        public DateTime Date { get; set; }

        public string FromAccount { get; set; } = string.Empty;

        public string ToAccount { get; set; } = string.Empty;

        public string CheckNumber { get; set; } = string.Empty;

        public (string Name, Color Color)? Flag { get; set; }

        public string Memo { get; set; } = string.Empty;

        public long Sum { get; set; }

        public bool Cleared { get; set; }
    }
}
