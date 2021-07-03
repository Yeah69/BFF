using MrMeeseeks.StaticDelegateGenerator;
using System;
using System.Runtime.CompilerServices;

[assembly: StaticDelegate(typeof(DateTime))]
[assembly: InternalsVisibleTo("BFF.Composition")]
namespace BFF.Persistence
{
    public static class AssemblyInfo
    {
    }
}