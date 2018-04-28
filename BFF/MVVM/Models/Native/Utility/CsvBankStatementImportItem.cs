using System;

namespace BFF.MVVM.Models.Native.Utility
{
    public interface ICsvBankStatementImportItem
    {
        DateTime Date { get; set; }

        string Payee { get; set; }

        string Memo { get; set; }

        long Sum { get; set; }
    }

    public class CsvBankStatementImportItem : ICsvBankStatementImportItem
    {
        public DateTime Date { get; set; }

        public string Payee { get; set; }

        public string Memo { get; set; }

        public long Sum { get; set; }
    }
}
