using System;
using System.Globalization;
using System.Windows.Media;
using BFF.Helper;

namespace BFF.Model.Conversion.YNAB
{
    class Transaction
    {
        public static readonly string CSVHeader = "\"Account\"	\"Flag\"	\"Check Number\"	\"Date\"	\"Payee\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Memo\"	\"Outflow\"	\"Inflow\"	\"Cleared\"	\"Running Balance\"";

        private string account;
        private Color flag;
        private int checkNumber;
        private DateTime date;
        private string payee;
        private string category;
        private string masterCategory;
        private string subCategory;
        private string memo;
        private double outflow;
        private double inflow;
        private bool cleared;
        private double runningBalance;

        public static void ToOutput(Transaction transaction)
        {
            Output.WriteLine("BEGIN YNAB transaction");
            Output.WriteLine("\tAccount: \t\t\t" + transaction.account);
            Output.WriteLine("\tFlag: \t\t\t\t" + transaction.flag);
            Output.WriteLine("\tCheckNumber: \t\t" + transaction.checkNumber);
            Output.WriteLine("\tDate: \t\t\t\t" + transaction.date);
            Output.WriteLine("\tPayee: \t\t\t\t" + transaction.payee);
            Output.WriteLine("\tCategory: \t\t\t" + transaction.category);
            Output.WriteLine("\tMaster Category: \t" + transaction.masterCategory);
            Output.WriteLine("\tSub Category: \t\t" + transaction.subCategory);
            Output.WriteLine("\tMemo: \t\t\t\t" + transaction.memo);
            Output.WriteLine("\tOutflow: \t\t\t" + transaction.outflow);
            Output.WriteLine("\tInflow: \t\t\t" + transaction.inflow);
            Output.WriteLine("\tCleared: \t\t\t" + transaction.cleared);
            Output.WriteLine("\tRunning Balance: \t" + transaction.runningBalance);
            Output.WriteLine("END YNAB transaction");
        }

        public static implicit operator Transaction(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            //todo: adjust conversion to regional codes (date, outflow, inflow, runningBalance)
            Transaction ret = new Transaction();
            ret.account = entries[0].Trim('"');
            switch (entries[1])
            {
                case "Red":
                    ret.flag = Colors.Red;
                    break;
                case "Orange":
                    ret.flag = Colors.Orange;
                    break;
                case "Yellow":
                    ret.flag = Colors.Yellow;
                    break;
                case "Green":
                    ret.flag = Colors.Green;
                    break;
                case "Blue":
                    ret.flag = Colors.Blue;
                    break;
                case "Purple":
                    ret.flag = Colors.Purple;
                    break;
                default:
                    ret.flag = Colors.Transparent;
                    break;
            }
            if(!int.TryParse(entries[2], out ret.checkNumber))
                ret.checkNumber = -1;
            ret.date = DateTime.ParseExact(entries[3], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            ret.payee = entries[4].Trim('"');
            ret.category = entries[5].Trim('"');
            ret.masterCategory = entries[6].Trim('"');
            ret.subCategory = entries[7].Trim('"');
            ret.memo = entries[8].Trim('"');
            ret.outflow = double.Parse(entries[9].TrimEnd('€'));
            ret.inflow = double.Parse(entries[10].TrimEnd('€'));
            ret.cleared = entries[11] == "C";
            ret.runningBalance = double.Parse(entries[12].TrimEnd('€'));
            return ret;
        }
    }
}
