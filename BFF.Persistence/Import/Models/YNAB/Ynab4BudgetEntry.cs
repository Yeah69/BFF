namespace BFF.Persistence.Import.Models.YNAB
{
    internal class Ynab4BudgetEntry
    {
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
            Ynab4BudgetEntry ret = new Ynab4BudgetEntry
            {
                Month = entries[0].Trim('"'),
                Category = entries[1].Trim('"'),
                MasterCategory = entries[2].Trim('"'),
                SubCategory = entries[3].Trim('"'),
                Budgeted = Ynab4CsvImportBase.ExtractLong(entries[4]),
                Outflow = Ynab4CsvImportBase.ExtractLong(entries[5]),
                CategoryBalance = Ynab4CsvImportBase.ExtractLong(entries[6])
            };
            return ret;
        }
    }
}
