using System;
using System.ComponentModel;

namespace BFF.Tests.Helper
{
    class NativeAssert
    {
        /// <summary>
        /// Verifies that the provided object has not raised <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// as a result of executing the given test code.
        /// </summary>
        /// <param name="object">The object which should not raise the notification</param>
        /// <param name="propertyName">The property name for which the notification should not be raised</param>
        /// <param name="testCode">The test code which should not cause the notification to be raised</param>
        /// <exception cref="DoesNotRaisePropertyChangedException">Thrown when the notification is raised</exception>
        public static void DoesNotRaisePropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));
            if (testCode == null)
                throw new ArgumentNullException(nameof(testCode));

            bool propertyChangeHappened = false;

            PropertyChangedEventHandler handler = (sender, args) => propertyChangeHappened |= string.IsNullOrEmpty(args.PropertyName) || propertyName.Equals(args.PropertyName, StringComparison.OrdinalIgnoreCase);

            @object.PropertyChanged += handler;

            try
            {
                testCode();
                if (propertyChangeHappened)
                    throw new DoesNotRaisePropertyChangedException(propertyName);
            }
            finally
            {
                @object.PropertyChanged -= handler;
            }
        }
    }
}
