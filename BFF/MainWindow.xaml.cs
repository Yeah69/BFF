using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.Properties;
using BFF.ViewModel;
using BFF.WPFStuff.UserControls;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TabItem = System.Windows.Controls.TabItem;

namespace BFF
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public DbSetting DbSettings { get; set; }

        //todo: Maybe not needed if gotten directly from DataContext (the ViewModel)
        public static readonly DependencyProperty ImportCommandProperty =
            DependencyProperty.Register(nameof(ImportCommand), typeof (ICommand), typeof (MainWindow),
                new PropertyMetadata((depObj, args) => 
                {
                    var mw = (MainWindow) depObj;
                    mw.ImportCommand = (ICommand) args.NewValue;
                }));

        private void changeTabs(object param, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Account account in args.NewItems)
                        AccountsTabControl.Items.Insert(AccountsTabControl.Items.Count - 1, createMetroTabItem(account));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    //todo: Check if this works
                    foreach (Account account in args.OldItems)
                    {
                        foreach (TabItem tabItem in AccountsTabControl.Items)
                        {
                            if(tabItem.DataContext == account)
                                AccountsTabControl.Items.Remove(tabItem);
                        }
                    }
                    break;
            }
        }

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

        private MainWindow()
        {
        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            DbSettings = SqLiteHelper.GetDbSetting();
            Resources["CurrencyCulture"] = DbSettings.CurrencyCulture;
            InitializeComponent();

            foreach (Account account in viewModel.AllAccounts)
                AccountsTabControl.Items.Insert(AccountsTabControl.Items.Count - 1, createMetroTabItem(account));
            viewModel.AllAccounts.CollectionChanged += changeTabs;

            SetBinding(ImportCommandProperty, nameof(MainWindowViewModel.ImportBudgetPlanCommand));

            Accent initialAccent = ThemeManager.GetAccent(Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(Settings.Default.MahApps_AppTheme);
            string initialLocalization = Settings.Default.Localization_Language;

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
            LanguageCombo.Items.Add("de-DE");
            LanguageCombo.Items.Add("en-US");
            LanguageCombo.SelectedItem = initialLocalization;

            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().OrderBy(x => x.Name))
            {
                CurrencyCombo.Items.Add(culture);
            }

            CurrencyCombo.SelectedItem = DbSettings.CurrencyCulture;

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(initialLocalization);
        }

        private void SettingsButt_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent accent = ThemeManager.GetAccent(Settings.Default.MahApps_Accent);
            ThemeManager.ChangeAppStyle(Application.Current, accent, ((ThemeWrap)ThemeCombo.SelectedItem).Theme);
            Settings.Default.MahApps_AppTheme = ((ThemeWrap)ThemeCombo.SelectedItem).Theme.Name;
            Settings.Default.Save();
        }

        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme theme = ThemeManager.GetAppTheme(Settings.Default.MahApps_AppTheme);
            ThemeManager.ChangeAppStyle(Application.Current, ((AccentWrap)AccentCombo.SelectedItem).Accent, theme);
            Settings.Default.MahApps_Accent = ((AccentWrap)AccentCombo.SelectedItem).Accent.Name;
            Settings.Default.Save();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string) LanguageCombo.SelectedItem;
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(language);
            Settings.Default.Localization_Language = language;
            Settings.Default.Save();
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

            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
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
                    Dispatcher.Invoke(() => ImportCommand.Execute(importDialogVM.Importable), DispatcherPriority.Background);
            };
            this.ShowMetroDialogAsync(importDialog);
        }

        private void CurrencyCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DbSettings.CurrencyCulture = (CultureInfo) (((ComboBox) sender).SelectedItem);
            refreshCurrencyVisuals();
        }

        private void refreshCurrencyVisuals()
        {
            NewAccount_StartingBalance.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            for (int i = 0; i < AccountsTabControl.Items.Count - 1; i++)
            {
                TitDataGrid titDataGrid = (TitDataGrid)((MetroTabItem)AccountsTabControl.Items[i]).Content;
                titDataGrid.refreshCurrencyVisuals();
            }
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
