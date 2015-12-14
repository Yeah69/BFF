using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
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
