namespace BFF.Model.Conversion.YNAB
{
    class Transaction
    {
        public static readonly string CSVHeader = "\"Account\"	\"Flag\"	\"Check Number\"	\"Date\"	\"Payee\"	\"Category\"	\"Master Category\"	\"Sub Category\"	\"Memo\"	\"Outflow\"	\"Inflow\"	\"Cleared\"	\"Running Balance\"";

        private string account;
        private string flag;
        private string checkNumber;
        private string date;
        private string payee;
        private string category;
        private string masterCategory;
        private string subCategory;
        private string memo;
        private string outflow;
        private string inflow;
        private string cleared;
        private string runningBalance;
    }
}
