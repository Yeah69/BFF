namespace BFF.Persistence.Import.Models.YNAB
{
    internal class Ynab4BudgetEntry
    {
        internal Ynab4BudgetEntry(
            string month,
            string category,
            string masterCategory,
            string subCategory,
            long budgeted,
            long outflow,
            long categoryBalance)
        {
            Month = month;
            Category = category;
            MasterCategory = masterCategory;
            SubCategory = subCategory;
            Budgeted = budgeted;
            Outflow = outflow;
            CategoryBalance = categoryBalance;
        }

        public string Month { get; set; }
        public string Category { get; set; }
        public string MasterCategory { get; set; }
        public string SubCategory { get; set; }
        public long Budgeted { get; set; }
        public long Outflow { get; set; }
        public long CategoryBalance { get; set; }
        
        public static readonly string CsvHeader = "\"Month\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Budgeted\"	\"Outflows\"	\"Category Balance\"";

        public static implicit operator Ynab4BudgetEntry(string csvLine)
        {
            string[] entries = csvLine.Split('\t');
            Ynab4BudgetEntry ret = new (
                entries[0].Trim('"'),
                entries[1].Trim('"'),
                entries[2].Trim('"'),
                entries[3].Trim('"'),
                Helper.ExtractLong(entries[4]),
                Helper.ExtractLong(entries[5]),
                Helper.ExtractLong(entries[6]));
            return ret;
        }
    }
}
