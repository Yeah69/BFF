using BFF.Persistence.Import;
using NLog;

namespace BFF.Persistence.Models.Import.YNAB
{
    internal class BudgetEntry
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Debug("BEGIN YNAB budget entry");
            Logger.Debug("\tMonth: \t\t\t\t{0}", budgetEntry.Month);
            Logger.Debug("\tCategory: \t\t\t{0}", budgetEntry.Category);
            Logger.Debug("\tMaster Category: \t{0}", budgetEntry.MasterCategory);
            Logger.Debug("\tSub Category: \t\t{0}", budgetEntry.SubCategory);
            Logger.Debug("\tInflow: \t\t\t{0}", budgetEntry.Budgeted);
            Logger.Debug("\tOutflow: \t\t\t{0}", budgetEntry.Outflow);
            Logger.Debug("\tRunning Balance: \t{0}", budgetEntry.CategoryBalance);
            Logger.Debug("END YNAB budget entry");
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
                Budgeted = Ynab4Import.ExtractLong(entries[4]),
                Outflow = Ynab4Import.ExtractLong(entries[5]),
                CategoryBalance = Ynab4Import.ExtractLong(entries[6])
            };
            return ret;
        }
    }
}
