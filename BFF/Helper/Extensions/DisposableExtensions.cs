using System;
using System.Reactive.Disposables;

namespace BFF.Helper.Extensions
{
    public static class DisposableExtensions
    {
        public static T AssignTo<T>(this T @this, SerialDisposable serialDisposable) where T : IDisposable
        {
            serialDisposable.Disposable = @this;
            return @this;
        } 
    }
}
