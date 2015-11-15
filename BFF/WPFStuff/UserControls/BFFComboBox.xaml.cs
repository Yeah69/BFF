using System;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for BFFComboBox.xaml
    /// </summary>
    public partial class BFFComboBox : UserControl
    {
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(BFFComboBox), new PropertyMetadata(null, SelectedItemChanged));

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(BFFComboBox), new PropertyMetadata(null, ItemsSourceChanged));

        public static DependencyProperty CreateNamedInstanceFuncProperty = DependencyProperty.Register(nameof(CreateNamedInstanceFunc), typeof(Func<string, object>), typeof(BFFComboBox), new PropertyMetadata(null, CreateNamedInstanceFunctionChanged));

        private static void SelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BFFComboBox)dependencyObject).SelectedItem = e.NewValue;
            }
        }

        private static void ItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) ((BFFComboBox)dependencyObject).ItemsSource = (IList)e.NewValue;
        }

        private static void CreateNamedInstanceFunctionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) ((BFFComboBox)dependencyObject).CreateNamedInstanceFunc = (Func<string, object>)e.NewValue;
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set
            {
                SetValue(SelectedItemProperty, value);
                if(!inEditMode) SelectedBox.Text = value?.ToString();
            }
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public Func<string, object> CreateNamedInstanceFunc
        {
            get { return (Func<string, object>)GetValue(CreateNamedInstanceFuncProperty); }
            set { SetValue(CreateNamedInstanceFuncProperty, value); }
        }

        private bool inEditMode = false;

        private bool focusedLostDisabled = false;

        private bool inListBox = false;

        private object oldSelectedItem;

        public BFFComboBox()
        {
            InitializeComponent();
        }

        private void StartEdit()
        {
            if (!inEditMode)
            {
                oldSelectedItem = SelectedItem;
                SelectedBox.BorderThickness = new Thickness(1.0);
                SelectedBox.IsReadOnly = false;
                SelectionList.ItemsSource = ItemsSource;
                SelectionList.SelectedItem = SelectedItem;
                SelectionPopup.IsOpen = true;
                inEditMode = true;
            }
        }

        private void FinishEdit()
        {
            if (inEditMode)
            {
                SelectedBox.BorderThickness = new Thickness(0.0);
                SelectedBox.IsReadOnly = true;
                SelectionPopup.IsOpen = false;
                AddButt.Visibility = Visibility.Collapsed;
                inEditMode = false;
                SelectedBox.Text = SelectedItem.ToString();
                GetBindingExpression(BFFComboBox.SelectedItemProperty).UpdateSource();
            }
        }

        public void AddObject()
        {
            object addedObject = CreateNamedInstanceFunc?.Invoke(SelectedBox.Text);
            SelectedItem = addedObject;
            FinishEdit();
        }

        private void SelectedBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (inEditMode)
            {
                IList filtered = new ArrayList();
                bool fullMatch = false;
                foreach (object item in ItemsSource)
                {
                    if (item.ToString().Contains(SelectedBox.Text))
                    {
                        filtered.Add(item);
                        if (item.ToString() == SelectedBox.Text) fullMatch = true;
                    }
                }
                AddButt.Visibility = fullMatch || string.IsNullOrEmpty(SelectedBox.Text) ? Visibility.Collapsed : Visibility.Visible;
                SelectionList.ItemsSource = filtered;
                if (SelectionList.SelectedItems.Count == 0 && filtered.Count > 0)
                    SelectionList.SelectedIndex = 0;
            }
        }

        private void SelectedBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StartEdit();
        }

        private void SelectionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!inEditMode) return;
            SelectionList.GetBindingExpression(ListBox.SelectedItemProperty).UpdateSource();
            FinishEdit();
        }

        private void SelectedBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (focusedLostDisabled) return;
            FinishEdit();
        }

        private void SelectedBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            int index;
            switch (e.Key)
            {
                case Key.Enter:
                    if (SelectionList.Items.Count > 0 && SelectionList.SelectedItem != null)
                    {
                        SelectedItem = SelectionList.SelectedItem;
                        FinishEdit();
                        break;
                    }
                    if (AddButt.Visibility == Visibility.Visible)
                        AddObject();
                    break;
                case Key.Escape:
                    SelectedItem = oldSelectedItem;
                    FinishEdit();
                    break;
                /*case Key.Down:
                        if (SelectionList.Items.Count > 0 && SelectionList.SelectedItem != null)
                        {
                            index = SelectionList.SelectedIndex + 1;
                            if (index >= SelectionList.Items.Count)
                                index = 0;
                            SelectionList.SelectedIndex = index;
                        }
                        break;
                    case Key.Up:
                        if (SelectionList.Items.Count > 0 && SelectionList.SelectedItem != null)
                        {
                            index = SelectionList.SelectedIndex - 1;
                            if (index < 0)
                                index = SelectionList.Items.Count - 1;
                            SelectionList.SelectedIndex = index;
                        }
                        break;*/
            }
        }

        private void AddButt_OnClick(object sender, RoutedEventArgs e)
        {
            AddObject();
        }

        private void AddButt_OnMouseEnter(object sender, MouseEventArgs e)
        {
            focusedLostDisabled = true;
        }

        private void AddButt_OnMouseLeave(object sender, MouseEventArgs e)
        {
            focusedLostDisabled = false;
        }

        private void SelectionList_OnMouseEnter(object sender, MouseEventArgs e)
        {
            focusedLostDisabled = true;
        }

        private void SelectionList_OnMouseLeave(object sender, MouseEventArgs e)
        {
            focusedLostDisabled = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectedItem = SelectionList.SelectedItem;
            FinishEdit();
        }
    }
}
