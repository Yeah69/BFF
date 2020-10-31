using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MrMeeseeks.Utility;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels
{
    public interface IViewModel : IObservableObject
    {
    }

    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
    }

    public interface INotifyingErrorViewModel : IViewModel, INotifyDataErrorInfo
    {
    }

    public abstract class NotifyingErrorViewModelBase : ViewModelBase, INotifyingErrorViewModel
    {
        private readonly IDictionary<string, IEnumerable<string>> _errors = new Dictionary<string, IEnumerable<string>>();

        public IEnumerable? GetErrors(string? propertyName)
        {
            if (propertyName is null) return null;
            return _errors.TryGetValue(propertyName, out var errors) 
                ? errors 
                : null;
        }

        protected void SetErrors(IEnumerable<string> errors, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null)
                _errors[propertyName] = errors;
        }

        protected void ClearErrors([CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null && _errors.ContainsKey(propertyName))
                _errors.Remove(propertyName);
        }

        protected void ClearAllErrors()
        {
            _errors.Clear();
        }

        protected void OnErrorChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors => _errors.Values.SelectMany(Basic.Identity).Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    }
}
