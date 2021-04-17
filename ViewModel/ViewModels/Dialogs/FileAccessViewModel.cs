using System;
using System.IO;
using BFF.Model.ImportExport;
using BFF.ViewModel.Helper;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using System.Reactive.Linq;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IFileAccessViewModel : IMainWindowDialogContentViewModel<IProjectFileAccessConfiguration>
    {
        ICommand BrowseCommand { get; }
        ICommand OkCommand { get; }
        string? Path { get; set; }
    }

    public abstract class FileAccessViewModel : MainWindowDialogContentViewModel<IProjectFileAccessConfiguration>, IFileAccessViewModel
    {
        private readonly Func<IPasswordProtectedFileAccessViewModel> _passwordProtectedFileAccessViewModelFactory;

        public FileAccessViewModel(
            // parameter
            string title,
            
            // dependencies
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration) : base(title)
        {
            _passwordProtectedFileAccessViewModelFactory = passwordProtectedFileAccessViewModelFactory;
            
            var okCommand = RxCommand.CallerDeterminedCanExecute(this.ObservePropertyChanged(nameof(Path))
                .Select(_ => Path is { } path && File.Exists(path)));

            okCommand
                .Observe
                .Subscribe(_ => TaskCompletionSource.SetResult(GenerateConfiguration()))
                .CompositeDisposalWith(CompositeDisposable);

            this.OkCommand = okCommand;
            
            IProjectFileAccessConfiguration GenerateConfiguration()
            {
                if (this.Path is null) throw new FileNotFoundException();

                return PasswordConfiguration is { IsEncryptionActive: true, Password: {} password}
                    ? createFileAccessConfiguration.CreateWithEncryption(Path, password)
                    : createFileAccessConfiguration.Create(Path);
            }
        }

        private string? _path;
        private IPasswordProtectedFileAccessViewModel? _passwordConfiguration;

        public ICommand OkCommand { get; }

        public string? Path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;

                PasswordConfiguration = _path?.EndsWith(".realm") ?? false
                    ? _passwordProtectedFileAccessViewModelFactory() 
                    : null;

                OnPropertyChanged();
            }
        }

        public abstract ICommand BrowseCommand { get; }

        public IPasswordProtectedFileAccessViewModel? PasswordConfiguration
        {
            get => _passwordConfiguration;
            private set => SetIfChangedAndRaise(ref _passwordConfiguration, value);
        }
    }

    public interface INewFileAccessViewModel : IFileAccessViewModel
    {
    }

    public sealed class NewFileAccessViewModel : FileAccessViewModel, INewFileAccessViewModel
    {
        public NewFileAccessViewModel(
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            Func<IBffSaveFileDialog> bffSaveFileDialogFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration,
            ILocalizer localizer) 
            : base(
                localizer.Localize("OpenSaveDialog_TitleNew"),
                passwordProtectedFileAccessViewModelFactory,
                createFileAccessConfiguration)
        {

            BrowseCommand = new RxRelayCommand(() =>
            {
                var bffSaveFileDialog = bffSaveFileDialogFactory();
                bffSaveFileDialog.Title = localizer.Localize("OpenSaveDialog_TitleNew");
                bffSaveFileDialog.Filter = localizer.Localize("OpenSaveDialog_Filter");
                bffSaveFileDialog.DefaultExt = "*.realm";
                if (bffSaveFileDialog.ShowDialog() == true)
                {
                    Path = bffSaveFileDialog.FileName;
                }
            });
        }

        public override ICommand BrowseCommand { get; }
    }

    public interface IOpenFileAccessViewModel : IFileAccessViewModel
    {
    }

    public sealed class OpenFileAccessViewModel : FileAccessViewModel, IOpenFileAccessViewModel
    {
        public OpenFileAccessViewModel(
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            Func<IBffOpenFileDialog> bffOpenFileDialogFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration,
            ILocalizer localizer)
            : base(
                localizer.Localize("OpenSaveDialog_TitleOpen"),
                passwordProtectedFileAccessViewModelFactory, 
                createFileAccessConfiguration)
        {

            BrowseCommand = new RxRelayCommand(() =>
            {
                var bffOpenFileDialog = bffOpenFileDialogFactory();
                bffOpenFileDialog.Title = localizer.Localize("OpenSaveDialog_TitleOpen");
                bffOpenFileDialog.Filter = localizer.Localize("OpenSaveDialog_Filter");
                bffOpenFileDialog.DefaultExt = "*.realm";
                if (bffOpenFileDialog.ShowDialog() == true)
                {
                    Path = bffOpenFileDialog.FileName;
                }
            });
        }

        public override ICommand BrowseCommand { get; }
    }
}
