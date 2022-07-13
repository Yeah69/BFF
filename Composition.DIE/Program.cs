using BFF.View.Wpf;
using System;
using System.Threading.Tasks;

namespace BFF.Composition.DIE
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var container = new Container();
            App app = container.Create();
            app.Run();
            Task.Run(() => container.DisposeAsync()).Wait();
        }
    }
}