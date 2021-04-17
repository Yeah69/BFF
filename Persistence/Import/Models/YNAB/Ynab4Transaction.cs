using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BFF.Persistence.Import.Models.YNAB
{
    internal class Ynab4Transaction
    {
        public string Account { get; set; } = string.Empty;

        public string Flag { get; set; } = string.Empty;

        public string CheckNumber { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Payee { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string MasterCategory { get; set; } = string.Empty;

        public string SubCategory { get; set; } = string.Empty;

        public string Memo { get; set; } = string.Empty;

        public long Outflow { get; set; }

        public long Inflow { get; set; }

        public bool Cleared { get; set; }

        private double RunningBalance { get; set; }

        public static readonly string CsvHeader = "\"Account\"	\"Flag\"	\"Check Number\"	\"Date\"	\"Payee\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Memo\"	\"Outflow\"	\"Inflow\"	\"Cleared\"	\"Running Balance\"";

        private static readonly Regex PayeePartsRegex = new(@"^(?<payeeStr>.+)?(( / )?Transfer : (?<accountName>.+))?$", RegexOptions.RightToLeft);

        private string ParseAccountFromPayee()
        {
            return PayeePartsRegex.Match(Payee).Groups["accountName"].Value;
        }
        
        public string ParsePayeeFromPayee()
        {
            return PayeePartsRegex.Match(Payee).Groups["payeeStr"].Value;
        }

        public string GetFromAccountName()
        {
            return this.Inflow - this.Outflow < 0 ? this.Account : this.ParseAccountFromPayee();
        }

        public string GetToAccountName()
        {
            return this.Inflow - this.Outflow < 0 ? this.ParseAccountFromPayee() : this.Account;
        }

        public static implicit operator Ynab4Transaction(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            
            Ynab4Transaction ret = new()
            {
                Account = entries[0].Trim('"'),
                Flag = entries[1],
                CheckNumber = entries[2],
                Date = DateTime.ParseExact(entries[3], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                Payee = entries[4].Trim('"'),
                Category = entries[5].Trim('"'),
                MasterCategory = entries[6].Trim('"'),
                SubCategory = entries[7].Trim('"'),
                Memo = entries[8].Trim('"'),
                Outflow = Helper.ExtractLong(entries[9]),
                Inflow = Helper.ExtractLong(entries[10]),
                Cleared = entries[11] == "C",
                RunningBalance = Helper.ExtractLong(entries[12]),
            };
            return ret;
        }

    }
}
