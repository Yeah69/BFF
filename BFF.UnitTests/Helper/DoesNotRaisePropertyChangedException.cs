using System.Globalization;
using Xunit.Sdk;

namespace BFF.Tests.Helper
{
    public class PropertyChangedException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PropertyChangedException"/> class. Call this constructor
        /// when no exception was thrown.
        /// </summary>
        /// <param name="propertyName">The name of the property that was expected to be changed.</param>
        public PropertyChangedException(string propertyName)
            : base(string.Format(CultureInfo.CurrentCulture, "NativeAssert.PropertyChangedException failure: PropertyChanged for property {0} was raised", propertyName))
        { }
    }
}
