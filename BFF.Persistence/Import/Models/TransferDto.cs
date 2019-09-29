using System;
using System.Drawing;

namespace BFF.Persistence.Import.Models
{
    internal class TransferDto
    {
        public DateTime Date { get; set; }

        public string FromAccount { get; set; }

        public string ToAccount { get; set; }

        public string CheckNumber { get; set; }

        public (string Name, Color Color)? Flag { get; set; }

        public string Memo { get; set; }

        public long Sum { get; set; }

        public bool Cleared { get; set; }
    }
}
