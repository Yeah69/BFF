using BFF.Core.IoC;
using System;

namespace BFF.Model.Contexts
{
    public interface IContext : IDisposable, ITransientScopeRoot
    {
        string Title { get; }
    }
}