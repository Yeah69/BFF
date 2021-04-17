using System;

namespace BFF.Model.Contexts
{
    public interface IContext : IDisposable
    {
        string Title { get; }
    }
}