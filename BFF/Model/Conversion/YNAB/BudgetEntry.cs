using BFF.Helper;

namespace BFF.Model.Conversion.YNAB
{
    class BudgetEntry
    {
        public static readonly string CSVHeader = "\"Month\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Budgeted\"	\"Outflows\"	\"Category Balance\"";

        private string month;
        private string category;
        private string masterCategory;
        private string subCategory;
        private double budgeted;
        private double outflow;
        private double categoryBalance;

        public static void ToOutput(BudgetEntry budgetEntry)
        {
            Output.WriteLine("BEGIN YNAB budget entry");
            Output.WriteLine("\tMonth: \t\t\t\t" + budgetEntry.month);
            Output.WriteLine("\tCategory: \t\t\t" + budgetEntry.category);
            Output.WriteLine("\tMaster Category: \t" + budgetEntry.masterCategory);
            Output.WriteLine("\tSub Category: \t\t" + budgetEntry.subCategory);
            Output.WriteLine("\tInflow: \t\t\t" + budgetEntry.budgeted);
            Output.WriteLine("\tOutflow: \t\t\t" + budgetEntry.outflow);
            Output.WriteLine("\tRunning Balance: \t" + budgetEntry.categoryBalance);
            Output.WriteLine("END YNAB budget entry");
        }

        public static implicit operator BudgetEntry(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            //todo: adjust conversion to regional codes (month, budgeted, outflow, categoryBalance)
            BudgetEntry ret = new BudgetEntry();
            ret.month = entries[0].Trim('"');
            ret.category = entries[1].Trim('"');
            ret.masterCategory = entries[2].Trim('"');
            ret.subCategory = entries[3].Trim('"');
            ret.budgeted = double.Parse(entries[4].TrimEnd('€'));
            ret.outflow = double.Parse(entries[5].TrimEnd('€'));
            ret.categoryBalance = double.Parse(entries[6].TrimEnd('€'));
            return ret;
        }
    }
}
