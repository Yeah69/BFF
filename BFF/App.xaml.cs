﻿using BFF.DB.SQLite;
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
            SqLiteHelper.CurrentDbName = "testDatabase";
            MainWindow mainWindow = new MainWindow();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            mainWindow.Show();
            mainWindow.DataContext = mainWindowViewModel;

        }
    }
}
