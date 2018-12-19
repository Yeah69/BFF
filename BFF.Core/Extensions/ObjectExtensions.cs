using System;

namespace BFF.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsInstanceOfType(this object @this, Type type) => type.IsInstanceOfType(@this);
    }
}
