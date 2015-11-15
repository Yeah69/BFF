﻿using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BFF.WPFStuff.Behaviors
{
    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
                                               SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox?.SelectedItem != null)
            {
                listBox.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        listBox.UpdateLayout();
                        if (listBox.SelectedItem != null)
                            listBox.ScrollIntoView(listBox.SelectedItem);
                    }));
            }
        }

        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }
    }
}
