using MrMeeseeks.StaticDelegateGenerator;
using System;
using System.Runtime.CompilerServices;

[assembly: StaticDelegate(typeof(DateTime))]
[assembly: InternalsVisibleTo("BFF.Composition")]
[assembly: InternalsVisibleTo("BFF")]
namespace BFF.Persistence
{
    public static class AssemblyInfo
    {
    }
}