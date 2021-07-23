using System;

namespace BFF.Composition.Wpf.Program
{
    public interface IRoot
    {
        (View.Wpf.App, IDisposable) WpfComposition();
    }

    public static class Root
    {
        public static IRoot Create() => new RootInner();
    }

    internal class RootInner : IRoot
    {
        public (View.Wpf.App, IDisposable) WpfComposition() => AutofacModule.ResolveWpfApp();

    }
}