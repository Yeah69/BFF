using BFF.Helper;
using BFF.Helper.Import;

namespace BFF.MVVM.Models.Conversion.YNAB
{
    class BudgetEntry
    {
        public string Month { get; set; }

        public string Category { get; set; }

        public string MasterCategory { get; set; }

        public string SubCategory { get; set; }

        public long Budgeted { get; set; }

        public long Outflow { get; set; }

        public long CategoryBalance { get; set; }

        public static readonly string CsvHeader = "\"Month\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Budgeted\"	\"Outflows\"	\"Category Balance\"";

        public static void ToOutput(BudgetEntry budgetEntry)
        {
            Output.WriteLine("BEGIN YNAB budget entry");
            Output.WriteLine("\tMonth: \t\t\t\t" + budgetEntry.Month);
            Output.WriteLine("\tCategory: \t\t\t" + budgetEntry.Category);
            Output.WriteLine("\tMaster Category: \t" + budgetEntry.MasterCategory);
            Output.WriteLine("\tSub Category: \t\t" + budgetEntry.SubCategory);
            Output.WriteLine("\tInflow: \t\t\t" + budgetEntry.Budgeted);
            Output.WriteLine("\tOutflow: \t\t\t" + budgetEntry.Outflow);
            Output.WriteLine("\tRunning Balance: \t" + budgetEntry.CategoryBalance);
            Output.WriteLine("END YNAB budget entry");
        }

        public static implicit operator BudgetEntry(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            BudgetEntry ret = new BudgetEntry
            {
                Month = entries[0].Trim('"'),
                Category = entries[1].Trim('"'),
                MasterCategory = entries[2].Trim('"'),
                SubCategory = entries[3].Trim('"'),
                Budgeted = YnabCsvImport.ExtractLong(entries[4]),
                Outflow = YnabCsvImport.ExtractLong(entries[5]),
                CategoryBalance = YnabCsvImport.ExtractLong(entries[6])
            };
            return ret;
        }
    }
}
