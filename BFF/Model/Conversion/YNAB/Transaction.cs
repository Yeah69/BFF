using System;
using System.Globalization;
using System.Windows.Media;
using BFF.Helper;

namespace BFF.Model.Conversion.YNAB
{
    class Transaction
    {

        #region Non-Static

        #region Properties

        public string Account { get; set; }

        public Color Flag { get; set; }

        public int CheckNumber { get; set; }

        public DateTime Date { get; set; }

        public string Payee { get; set; }

        public string Category { get; set; }

        public string MasterCategory { get; set; }

        public string SubCategory { get; set; }

        public string Memo { get; set; }

        public double Outflow { get; set; }

        public double Inflow { get; set; }

        public bool Cleared { get; set; }

        public double RunningBalance { get; set; }

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        public static readonly string CsvHeader = "\"Account\"	\"Flag\"	\"Check Number\"	\"Date\"	\"Payee\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Memo\"	\"Outflow\"	\"Inflow\"	\"Cleared\"	\"Running Balance\"";

        #endregion

        #region Static Methods

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
            //todo: adjust conversion to regional codes (date, outflow, inflow, runningBalance)
            Color tempColor;
            switch (entries[1])
            {
                case "Red":
                    tempColor = Colors.Red;
                    break;
                case "Orange":
                    tempColor = Colors.Orange;
                    break;
                case "Yellow":
                    tempColor = Colors.Yellow;
                    break;
                case "Green":
                    tempColor = Colors.Green;
                    break;
                case "Blue":
                    tempColor = Colors.Blue;
                    break;
                case "Purple":
                    tempColor = Colors.Purple;
                    break;
                default:
                    tempColor = Colors.Transparent;
                    break;
            }
            int temp;
            if (!int.TryParse(entries[2], out temp))
                temp = -1;
            Transaction ret = new Transaction
            {
                Account = entries[0].Trim('"'),
                Flag = tempColor,
                CheckNumber = temp,
                Date = DateTime.ParseExact(entries[3], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                Payee = entries[4].Trim('"'),
                Category = entries[5].Trim('"'),
                MasterCategory = entries[6].Trim('"'),
                SubCategory = entries[7].Trim('"'),
                Memo = entries[8].Trim('"'),
                Outflow = double.Parse(entries[9].TrimEnd('€')),
                Inflow = double.Parse(entries[10].TrimEnd('€')),
                Cleared = entries[11] == "C",
                RunningBalance = double.Parse(entries[12].TrimEnd('€'))
            };
            return ret;
        }

        #endregion

        #endregion
		
	
        

        

        
    }
}
