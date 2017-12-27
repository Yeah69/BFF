﻿using System;
using System.Globalization;
using BFF.Helper;
using BFF.Helper.Import;

namespace BFF.MVVM.Models.Conversion.YNAB
{
    public class Transaction
    {
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
            Output.WriteLine("BEGIN YNAB transaction");
            Output.WriteLine("\tAccount: \t\t\t" + transaction.Account);
            Output.WriteLine("\tFlag: \t\t\t\t" + transaction.Flag);
            Output.WriteLine("\tCheckNumber: \t\t" + transaction.CheckNumber);
            Output.WriteLine("\tDate: \t\t\t\t" + transaction.Date);
            Output.WriteLine("\tPayee: \t\t\t\t" + transaction.Payee);
            Output.WriteLine("\tCategory: \t\t\t" + transaction.Category);
            Output.WriteLine("\tMaster Category: \t" + transaction.MasterCategory);
            Output.WriteLine("\tSub Category: \t\t" + transaction.SubCategory);
            Output.WriteLine("\tMemo: \t\t\t\t" + transaction.Memo);
            Output.WriteLine("\tOutflow: \t\t\t" + transaction.Outflow);
            Output.WriteLine("\tInflow: \t\t\t" + transaction.Inflow);
            Output.WriteLine("\tCleared: \t\t\t" + transaction.Cleared);
            Output.WriteLine("\tRunning Balance: \t" + transaction.RunningBalance);
            Output.WriteLine("END YNAB transaction");
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
                Outflow = YnabCsvImport.ExtractLong(entries[9]),
                Inflow = YnabCsvImport.ExtractLong(entries[10]),
                Cleared = entries[11] == "C",
                RunningBalance = YnabCsvImport.ExtractLong(entries[12]),
            };
            return ret;
        }

    }
}
