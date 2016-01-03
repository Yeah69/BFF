using System;
using System.Windows.Input;

namespace BFF.Helper
{
    class TreeComboBoxHelper
    {
        public static Action<object, MouseButtonEventArgs> blah =
            (sender, args) => TreeView_MouseLeftButtonDown(sender, args); 

        public static void TreeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            int i = 0;
        }
    }
}
