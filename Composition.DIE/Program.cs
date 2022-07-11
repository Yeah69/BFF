using BFF.Composition.DIE;
using System;

namespace BFF.Composition.Wpf.Program
{
    public class Program
    {
        [STAThread]
        public static void Main()
        { 
            var container = new Container();
            /*var (app, disposable) = Root.Create().WpfComposition();
            try
            {
                app.Run();
            }
            finally
            {
                disposable.Dispose();
            }*/
        }
    }
}