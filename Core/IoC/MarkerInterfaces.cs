using System;
using System.Threading.Tasks;

namespace BFF.Core.IoC
{
    public interface IContainerInstance { }
    public interface ITransientScopeInstance { }
    public interface IScopeInstance { }
    public interface ITransientScopeRoot { }
    public interface IScopeRoot { }
    public interface ITransient : IDisposable { }
    public interface ISyncTransient { }
    public interface IAsyncTransient { }
    public interface IDecorator<T> { }
    public interface IComposite<T> { }
    public interface IInitializer
    {
        void Initialize();
    }
    public interface ITaskInitializer
    {
        Task InitializeAsync();
    }
    public interface IValueTaskInitializer
    {
        ValueTask InitializeAsync();
    }
}