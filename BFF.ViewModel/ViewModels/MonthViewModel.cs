using System;
using System.ComponentModel;
using BFF.Core.Helper;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels
{
    public interface IMonthViewModel : IObservableObject
    {
        int Number { get; }
        string Name { get; }
    }

    internal class MonthViewModel : ViewModelBase, IMonthViewModel, IDisposable
    {
        private readonly IBffSettings _bffSettings;
        private readonly IDisposable _subscription;
        public int Number { get; }
        
        public string Name => 
            new DateTime(1, this.Number, 1)
                .ToString(_bffSettings.Culture_DefaultDateLong 
                    ? "MMMM" 
                    : "MMM");

        public MonthViewModel(
            // parameters
            int monthNumber,
            
            // dependencies
            IBffSettings bffSettings,
            IBackendCultureManager cultureManager)
        {
            _bffSettings = bffSettings;
            this.Number = monthNumber;

            _subscription = cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.RefreshCurrency:
                    case CultureMessage.Refresh:
                        break;
                    case CultureMessage.RefreshDate:
                        this.OnPropertyChanged(nameof(Name));
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            });
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}