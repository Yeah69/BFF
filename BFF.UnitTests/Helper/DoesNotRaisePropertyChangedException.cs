using System.Globalization;
using Xunit.Sdk;

namespace BFF.Tests.Helper
{
    public class DoesNotRaisePropertyChangedException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotRaisePropertyChangedException"/> class. Call this constructor
        /// when no exception was thrown.
        /// </summary>
        /// <param name="propertyName">The name of the property that was not expected to raise a notification.</param>
        public DoesNotRaisePropertyChangedException(string propertyName)
            : base(string.Format(CultureInfo.CurrentCulture, "NativeAssert.PropertyChangedException failure: PropertyChanged for property {0} was raised", propertyName))
        { }
    }
}
