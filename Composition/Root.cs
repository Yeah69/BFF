using System;

namespace BFF.Composition
{
    public interface IRoot
    {
        (View.Wpf.App, IDisposable) WpfComposition();
        (View.Avalonia.App, IDisposable) AvaloniaComposition();
    }

    public static class Root
    {
        public static IRoot Create() => new RootInner();
    }

    internal class RootInner : IRoot
    {
        public (View.Wpf.App, IDisposable) WpfComposition() => AutofacModule.ResolveWpfApp();

        public (View.Avalonia.App, IDisposable) AvaloniaComposition()
        {
            throw new System.NotImplementedException();
        }
    }
}