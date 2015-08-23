using BFF.Helper;

namespace BFF.Model.Conversion.YNAB
{
    class BudgetEntry
    {
        #region Non-Static

        #region Properties

        public string Month { get; set; }

        public string Category { get; set; }

        public string MasterCategory { get; set; }

        public string SubCategory { get; set; }

        public double Budgeted { get; set; }

        public double Outflow { get; set; }

        public double CategoryBalance { get; set; }

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        public static readonly string CSVHeader = "\"Month\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Budgeted\"	\"Outflows\"	\"Category Balance\"";

        #endregion

        #region Static Methods

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
            //todo: adjust conversion to regional codes (month, budgeted, outflow, categoryBalance)
            BudgetEntry ret = new BudgetEntry();
            ret.Month = entries[0].Trim('"');
            ret.Category = entries[1].Trim('"');
            ret.MasterCategory = entries[2].Trim('"');
            ret.SubCategory = entries[3].Trim('"');
            ret.Budgeted = double.Parse(entries[4].TrimEnd('€'));
            ret.Outflow = double.Parse(entries[5].TrimEnd('€'));
            ret.CategoryBalance = double.Parse(entries[6].TrimEnd('€'));
            return ret;
        }

        #endregion

        #endregion
    }
}
