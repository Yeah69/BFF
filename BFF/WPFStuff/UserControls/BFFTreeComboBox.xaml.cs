using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for BFFComboBox.xaml
    /// </summary>
    public partial class BFFTreeComboBox : UserControl
    {
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(BFFTreeComboBox), new PropertyMetadata(null, SelectedItemChanged));

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(BFFTreeComboBox), new PropertyMetadata(null, ItemsSourceChanged));

        public static DependencyProperty CreateNamedInstanceFuncProperty = DependencyProperty.Register(nameof(CreateNamedInstanceFunc), typeof(Func<string, object>), typeof(BFFTreeComboBox), new PropertyMetadata(null, CreateNamedInstanceFunctionChanged));

        public static DependencyProperty SymbolRectangleProperty = DependencyProperty.Register(nameof(SymbolRectangle), typeof(Rectangle), typeof(BFFTreeComboBox), new PropertyMetadata(null, SymbolRectangelChanged));

        private static void SelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) ((BFFTreeComboBox)dependencyObject).SelectedItem = e.NewValue;
        }

        private static void ItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) ((BFFTreeComboBox)dependencyObject).ItemsSource = (IList)e.NewValue;
        }

        private static void CreateNamedInstanceFunctionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) ((BFFTreeComboBox)dependencyObject).CreateNamedInstanceFunc = (Func<string, object>)e.NewValue;
        }

        private static void SymbolRectangelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
                ((BFFTreeComboBox)dependencyObject).SymbolRectangle = (Rectangle)e.NewValue;
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

        public Rectangle SymbolRectangle
        {
            get { return (Rectangle)GetValue(SymbolRectangleProperty); }
            set { SetValue(SymbolRectangleProperty, value); }
        }

        private bool inEditMode = false;

        private bool focusedLostDisabled = false;

        private bool inListBox = false;

        private object oldSelectedItem;

        public BFFTreeComboBox()
        {
            InitializeComponent();
        }

        private void StartEdit()
        {
            if (!inEditMode)
            {
                oldSelectedItem = SelectedItem;
                SelectedBoxBorder.Visibility = Visibility.Visible;
                SelectedBox.IsReadOnly = false;
                SelectionTree.ItemsSource = ItemsSource;
                // todo => SelectionTree.SelectedItem = SelectedItem;
                SelectionPopup.IsOpen = true;
                inEditMode = true;
            }
        }

        private void FinishEdit()
        {
            if (inEditMode)
            {
                SelectedBoxBorder.Visibility = Visibility.Hidden;
                SelectedBox.IsReadOnly = true;
                SelectionPopup.IsOpen = false;
                AddButt.Visibility = Visibility.Collapsed;
                inEditMode = false;
                SelectedBox.Text = SelectedItem.ToString();
                GetBindingExpression(SelectedItemProperty)?.UpdateSource();
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
                AddButt.Visibility = CreateNamedInstanceFunc == null || fullMatch || string.IsNullOrEmpty(SelectedBox.Text)  ? Visibility.Collapsed : Visibility.Visible;
                SelectionTree.ItemsSource = filtered;
                // todo => if (SelectionTree.SelectedItems.Count == 0 && filtered.Count > 0)
                // todo => SelectionTree.SelectedIndex = 0;
            }
        }

        private void SelectedBox_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            StartEdit();
        }

        private void SelectedBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (focusedLostDisabled) return;
            FinishEdit();
        }

        private void SelectedBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            int index;
            switch (e.Key)
            {
                case Key.Enter:
                    if (SelectionTree.Items.Count > 0 && SelectionTree.SelectedItem != null)
                    {
                        SelectedItem = SelectionTree.SelectedItem;
                        FinishEdit();
                        break;
                    }
                    if (AddButt.Visibility == Visibility.Visible)
                        AddObject();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    SelectedItem = oldSelectedItem;
                    FinishEdit();
                    e.Handled = true;
                    break;
                case Key.Down:
                        if (SelectionTree.Items.Count > 0 && SelectionTree.SelectedItem != null)
                        {
                        // todo => index = SelectionTree.SelectedIndex + 1;
                        // todo => if (index >= SelectionTree.Items.Count)
                        // todo => index = 0;
                        // todo => SelectionTree.SelectedIndex = index;
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (SelectionTree.Items.Count > 0 && SelectionTree.SelectedItem != null)
                    {
                        // todo => index = SelectionTree.SelectedIndex - 1;
                        // todo => if (index < 0)
                        // todo => index = SelectionTree.Items.Count - 1;
                        // todo => SelectionTree.SelectedIndex = index;
                    }
                    e.Handled = true;
                break;
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
            SelectedItem = SelectionTree.SelectedItem;
            FinishEdit();
        }

        private void SelectedBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if(!inEditMode)
                SelectedBoxBorder.Visibility= Visibility.Visible;
        }

        private void SelectedBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(!inEditMode)
                SelectedBoxBorder.Visibility = Visibility.Hidden;
        }
    }
}
