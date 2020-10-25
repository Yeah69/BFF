using System.Windows;

namespace BFF.View.Views
{
    public partial class StatusCheckMark
    {
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register(
            nameof(Condition),
            typeof(bool),
            typeof(StatusCheckMark),
            new PropertyMetadata(default(bool)));

        

        public bool Condition
        {
            get => (bool) GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        public StatusCheckMark()
        {
            InitializeComponent();
        }
    }
}
