using System.Windows;
using BFF.MongoDb;

namespace BFF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MongoDbHelper.ConnectToMongoDb(null, null);
            MongoDbHelper.ImportYNABTransactionsCSVToDB(@"D:\Dropbox\YNAB\Exports\Yeah as of 2015-08-14 640 PM-Register.csv");
        }
    }
}
