using System.Reflection;
using System.Windows;
using BFF.DB.SQLite;
using BFF.ViewModel;

namespace BFF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\') + 1);
            SqLiteHelper.OpenDatabase($"{assemblyPath}testDatabase.sqlite");
            MainWindow mainWindow = new MainWindow();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            mainWindow.Show();
            mainWindow.DataContext = mainWindowViewModel;

        }
    }
}
