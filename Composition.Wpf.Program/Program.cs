using System;

namespace BFF.Composition.Wpf.Program
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var (app, disposable) = Root.Create().WpfComposition();
            try
            {
                app.Run();
            }
            finally
            {
                disposable.Dispose();
            }
        }
    }
}