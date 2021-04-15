using BFF.Core.IoC;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IMainWindowDialogViewModel
    {
        string Title { get; }
        IMainWindowDialogContentViewModel Content { get; }
    }

    internal class MainWindowDialogViewModel : IMainWindowDialogViewModel
    {
        public MainWindowDialogViewModel(
            IMainWindowDialogContentViewModel content) =>
            Content = content;

        public string Title => Content.Title;
        public IMainWindowDialogContentViewModel Content { get; }
    }

    public interface IMainWindowDialogContentViewModel : IViewModel
    {
        public string Title { get; }
        ICommand CancelCommand { get; }
    }

    public interface IMainWindowDialogContentViewModel<T> : IMainWindowDialogContentViewModel
    {
        Task<T> Result { get; }
    }

    public abstract class MainWindowDialogContentViewModel<T> : ViewModelBase, IMainWindowDialogContentViewModel<T>, IOncePerApplication
    {
        protected readonly TaskCompletionSource<T> TaskCompletionSource = new();
        protected readonly CompositeDisposable CompositeDisposable = new();
        
        public MainWindowDialogContentViewModel(string title)
        {
            Title = title;
            var cancelCommand = RxCommand.CanAlwaysExecute().CompositeDisposalWith(CompositeDisposable);
            cancelCommand.Observe
                .Subscribe(_ => TaskCompletionSource.SetCanceled())
                .CompositeDisposalWith(CompositeDisposable);
            CancelCommand = cancelCommand;

            TaskCompletionSource
                .Task
                .ContinueWith(_ => CompositeDisposable.Dispose());
        }

        public string Title { get; }
        public Task<T> Result => TaskCompletionSource.Task;
        public ICommand CancelCommand { get; }
    }
}