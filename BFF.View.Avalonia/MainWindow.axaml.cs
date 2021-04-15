using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BFF.View.Avalonia
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            void InitializeComponent()
            {
                AvaloniaXamlLoader.Load(this);
            }
        }
    }
}
