namespace BFF.View.Avalonia
{
    internal interface IAppContext
    {
        MainWindow MainWindow { get; }
    }

    internal class AppContext : IAppContext
    {
        public AppContext(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public MainWindow MainWindow { get; }
    }
}