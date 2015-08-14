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

            MongoDbHelper.testMongoDb();
        }
    }
}
