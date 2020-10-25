using System.Windows.Forms;
using BFF.ViewModel.Helper;

namespace BFF.View.Helper
{
    class BffSystemInformation : IBffSystemInformation
    {
        public double VirtualScreenRight => SystemInformation.VirtualScreen.Right;

        public double VirtualScreenLeft => SystemInformation.VirtualScreen.Left;
        public double VirtualScreenTop => SystemInformation.VirtualScreen.Top;
        public double VirtualScreenBottom => SystemInformation.VirtualScreen.Bottom;
    }
}
