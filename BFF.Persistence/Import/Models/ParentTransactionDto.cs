using System;
using System.Collections.Generic;
using System.Drawing;

namespace BFF.Persistence.Import.Models
{
    internal class ParentTransactionDto
    {
        public DateTime Date { get; set; }

        public string Account { get; set; }

        public string Payee { get; set; }

        public string CheckNumber { get; set; }

        public (string Name, Color Color)? Flag { get; set; }

        public string Memo { get; set; }

        public bool Cleared { get; set; }

        public IReadOnlyList<SubTransactionDto> SubTransactions { get; set; }
    }
}
