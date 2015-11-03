using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.Properties;
using BFF.ViewModel;
using BFF.WPFStuff.UserControls;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace BFF
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly DependencyProperty ImportCommandProperty =
            DependencyProperty.Register(nameof(ImportCommand), typeof (ICommand), typeof (MainWindow),
                new PropertyMetadata((depObj, args) => 
                {
                    var mw = (MainWindow) depObj;
                    mw.ImportCommand = (ICommand) args.NewValue;
                }));
        public static readonly DependencyProperty AllAccountsProperty =
            DependencyProperty.Register(nameof(AllAccounts), typeof(ObservableCollection<Account>), typeof(MainWindow),
                new PropertyMetadata((depObj, args) =>
                {
                    MetroTabControl accountsTabControl = ((MainWindow)depObj).AccountsTabControl;
                    ObservableCollection<Account> oldAccounts = (ObservableCollection<Account>)args.OldValue;
                    ObservableCollection<Account> newAccounts = (ObservableCollection<Account>)args.NewValue;
                    if (newAccounts != null)
                    {
                        if (oldAccounts == null) // if oldAccounts not initialized
                            foreach (Account account in newAccounts)
                                accountsTabControl.Items.Insert(accountsTabControl.Items.Count, createMetroTabItem(account));
                        else
                        {
                            foreach (Account account in newAccounts.Where(account => !oldAccounts.Contains(account)))
                            {
                                accountsTabControl.Items.Insert(accountsTabControl.Items.Count, createMetroTabItem(account));
                            }
                            foreach (Account account in oldAccounts.Where(account => !newAccounts.Contains(account)))
                            {
                                foreach (MetroTabItem metroTabItem in accountsTabControl.Items.Cast<MetroTabItem>().Where(metroTabItem => account == metroTabItem.DataContext))
                                {
                                    accountsTabControl.Items.Remove(metroTabItem);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (MetroTabItem metroTabItem in accountsTabControl.Items.Cast<MetroTabItem>().Where(metroTabItem => metroTabItem.DataContext is Account))
                        {
                            accountsTabControl.Items.Remove(metroTabItem);
                            break;
                        }
                    }
                    ((MainWindow) depObj).AllAccounts = newAccounts;
                }));

        private static MetroTabItem createMetroTabItem(Account account)
        {
            MetroTabItem tabItem = new MetroTabItem
            {
                DataContext = account,
                Content = new TitDataGrid { DataContext = new TitViewModel(account) }
            };
            tabItem.SetBinding(HeaderedContentControl.HeaderProperty, nameof(Account.Name));
            return tabItem;
        }

        public ICommand ImportCommand
        {
            get { return (ICommand) GetValue(ImportCommandProperty); }
            set { SetValue(ImportCommandProperty, value); }
        }

        public ObservableCollection<Account> AllAccounts
        {
            get { return (ObservableCollection<Account>)GetValue(AllAccountsProperty); }
            set { SetValue(AllAccountsProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.SetBinding(MainWindow.ImportCommandProperty, nameof(MainWindowViewModel.ImportBudgetPlanCommand));
            this.SetBinding(MainWindow.AllAccountsProperty, nameof(MainWindowViewModel.AllAccounts));

            Accent initialAccent = ThemeManager.GetAccent(Properties.Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(Properties.Settings.Default.MahApps_AppTheme);
            string initialLocalization = Properties.Settings.Default.Localization_Language;

            foreach (AppTheme theme in ThemeManager.AppThemes.OrderBy(x => x.Name))
            {
                ThemeWrap current = new ThemeWrap { Theme = theme, WhiteBrush = (SolidColorBrush) theme.Resources["WhiteBrush"] };
                ThemeCombo.Items.Add(current);
                if (theme == initialAppTheme)
                    ThemeCombo.SelectedItem = current;
            }
            foreach (Accent accent in ThemeManager.Accents.OrderBy(x => x.Name))
            {
                AccentWrap current = new AccentWrap { Accent = accent, AccentColorBrush = (SolidColorBrush) accent.Resources["AccentColorBrush"] };
                AccentCombo.Items.Add(current);
                if (accent == initialAccent)
                    AccentCombo.SelectedItem = current;
            }
            LanguageCombo.Items.Add("de");
            LanguageCombo.Items.Add("en");
            LanguageCombo.SelectedItem = initialLocalization;

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(initialLocalization);

            //YnabCsvImport.ImportYnabTransactionsCsvtoDb(@"D:\Private\YNABExports\Yeah as of 2015-08-14 640 PM-Register.csv",
            //                                            @"D:\Private\YNABExports\Yeah as of 2015-08-14 640 PM-Budget.csv",
            //                                            @"testDatabase");
        }

        private void SettingsButt_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent accent = ThemeManager.GetAccent(Properties.Settings.Default.MahApps_Accent);
            ThemeManager.ChangeAppStyle(Application.Current, accent, ((ThemeWrap)ThemeCombo.SelectedItem).Theme);
            Properties.Settings.Default.MahApps_AppTheme = ((ThemeWrap)ThemeCombo.SelectedItem).Theme.Name;
            Properties.Settings.Default.Save();
        }

        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme theme = ThemeManager.GetAppTheme(Properties.Settings.Default.MahApps_AppTheme);
            ThemeManager.ChangeAppStyle(Application.Current, ((AccentWrap)AccentCombo.SelectedItem).Accent, theme);
            Properties.Settings.Default.MahApps_Accent = ((AccentWrap)AccentCombo.SelectedItem).Accent.Name;
            Properties.Settings.Default.Save();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string) LanguageCombo.SelectedItem;
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(language);
            Properties.Settings.Default.Localization_Language = language;
            Properties.Settings.Default.Save();
        }

        private void FileButt_Click(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = true;
        }

        private void Close_FileFlyout(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = false;
        }

        private void Open_ImportDialog(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = false;

            this.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            //this.ShowInputAsync("Input", "Blaha");
            ImportDialogViewModel importDialogVM = new ImportDialogViewModel
            {
                Importable = new YnabCsvImport
                {
                    TransactionPath = Settings.Default.Import_YnabCsvTransaction,
                    BudgetPath = Settings.Default.Import_YnabCsvBudget,
                    SavePath = Settings.Default.Import_SavePath
                }
            };
            ImportDialog importDialog = new ImportDialog{ DataContext = importDialogVM };
            importDialog.ButtCancel.Click += (o, args) => this.HideMetroDialogAsync(importDialog);
            importDialog.ButtImport.Click += (o, args) =>
            {
                Settings.Default.Import_YnabCsvTransaction = ((YnabCsvImport)importDialogVM.Importable).TransactionPath;
                Settings.Default.Import_YnabCsvBudget = ((YnabCsvImport)importDialogVM.Importable).BudgetPath;
                Settings.Default.Import_SavePath = importDialogVM.Importable.SavePath;
                Settings.Default.Save();
                this.HideMetroDialogAsync(importDialog);
                //string savePath = importDialogVM.Importable.Import();
                //SqLiteHelper.OpenDatabase(savePath);
                if(ImportCommand.CanExecute(importDialogVM.Importable))
                    this.Dispatcher.Invoke(() => ImportCommand.Execute(importDialogVM.Importable), DispatcherPriority.Background);
            };
            this.ShowMetroDialogAsync(importDialog);
        }
    }

    internal class ThemeWrap
    {
        public AppTheme Theme { get; set; }
        public SolidColorBrush WhiteBrush { get; set; }

        public override string ToString()
        {
            return Theme.Name;
        }
    }

    internal class AccentWrap
    {
        public Accent Accent { get; set; }
        public SolidColorBrush AccentColorBrush { get; set; }
    }
}
