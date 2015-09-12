using BFF.Helper.Conversion;

namespace BFF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            YnabConversion.ImportYnabTransactionsCsvtoDb(@"D:\Dropbox\YNAB\Exports\Yeah as of 2015-08-14 640 PM-Register.csv", 
                                                        @"D:\Dropbox\YNAB\Exports\Yeah as of 2015-08-14 640 PM-Budget.csv",
                                                        @"testDatabase");
        }
    }
}
