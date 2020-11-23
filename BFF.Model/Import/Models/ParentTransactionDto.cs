using System;
using System.Collections.Generic;
using System.Drawing;

namespace BFF.Model.Import.Models
{
    public class ParentTransactionDto
    {
        public DateTime Date { get; set; }

        public string Account { get; set; } = string.Empty;

        public string Payee { get; set; } = string.Empty;

        public string CheckNumber { get; set; } = string.Empty;

        public (string Name, Color Color)? Flag { get; set; }

        public string Memo { get; set; } = string.Empty;

        public bool Cleared { get; set; }

        public IReadOnlyList<SubTransactionDto>? SubTransactions { get; set; }
    }
}
