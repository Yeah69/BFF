using System;
using System.Globalization;
using BFF.Persistence.Import;
using NLog;

namespace BFF.Persistence.Models.Import.YNAB
{
    internal class Transaction
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Account { get; set; }

        public string Flag { get; set; }

        public string CheckNumber { get; set; }

        public DateTime Date { get; set; }

        public string Payee { get; set; }

        public string Category { get; set; }

        public string MasterCategory { get; set; }

        public string SubCategory { get; set; }

        public string Memo { get; set; }

        public long Outflow { get; set; }

        public long Inflow { get; set; }

        public bool Cleared { get; set; }

        public double RunningBalance { get; set; }

        public static readonly string CsvHeader = "\"Account\"	\"Flag\"	\"Check Number\"	\"Date\"	\"Payee\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Memo\"	\"Outflow\"	\"Inflow\"	\"Cleared\"	\"Running Balance\"";
        
        public static void ToOutput(Transaction transaction)
        {
            Logger.Debug("BEGIN YNAB transaction");
            Logger.Debug("\tAccount: \t\t\t{0}", transaction.Account);
            Logger.Debug("\tFlag: \t\t\t\t{0}", transaction.Flag);
            Logger.Debug("\tCheckNumber: \t\t{0}", transaction.CheckNumber);
            Logger.Debug("\tDate: \t\t\t\t{0}", transaction.Date);
            Logger.Debug("\tPayee: \t\t\t\t{0}", transaction.Payee);
            Logger.Debug("\tCategory: \t\t\t{0}", transaction.Category);
            Logger.Debug("\tMaster Category: \t{0}", transaction.MasterCategory);
            Logger.Debug("\tSub Category: \t\t{0}", transaction.SubCategory);
            Logger.Debug("\tMemo: \t\t\t\t{0}", transaction.Memo);
            Logger.Debug("\tOutflow: \t\t\t{0}", transaction.Outflow);
            Logger.Debug("\tInflow: \t\t\t{0}", transaction.Inflow);
            Logger.Debug("\tCleared: \t\t\t{0}", transaction.Cleared);
            Logger.Debug("\tRunning Balance: \t{0}", transaction.RunningBalance);
            Logger.Debug("END YNAB transaction");
        }

        public static implicit operator Transaction(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            
            Transaction ret = new Transaction
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
                Outflow = Ynab4Import.ExtractLong(entries[9]),
                Inflow = Ynab4Import.ExtractLong(entries[10]),
                Cleared = entries[11] == "C",
                RunningBalance = Ynab4Import.ExtractLong(entries[12]),
            };
            return ret;
        }

    }
}
