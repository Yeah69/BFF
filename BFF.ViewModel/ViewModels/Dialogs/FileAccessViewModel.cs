using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.ViewModel.Helper;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IFileAccessViewModel : IOkCancelDialogViewModel
    {
        IRxRelayCommand BrowseCommand { get; set; }
        string? Path { get; set; }
        IPasswordProtectedFileAccessViewModel? PasswordConfiguration { get; }

        IFileAccessConfiguration GenerateConfiguration();
    }

    public abstract class FileAccessViewModel : OkCancelDialogViewModel
    {
        private readonly Func<IPasswordProtectedFileAccessViewModel> _passwordProtectedFileAccessViewModelFactory;
        private readonly ICreateFileAccessConfiguration _createFileAccessConfiguration;

        public FileAccessViewModel(
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration)
        {
            _passwordProtectedFileAccessViewModelFactory = passwordProtectedFileAccessViewModelFactory;
            _createFileAccessConfiguration = createFileAccessConfiguration;
            
            var okCommandCompositeDisposable = new CompositeDisposable().SerializeDisposalWith(OkCommandSerialDisposable);
            var okCommand = this.ObservePropertyChanged(nameof(Path))
                .Select(_ => Path is {} path && File.Exists(path))
                .AsCanExecuteForRxCommand(false)
                .CompositeDisposalWith(okCommandCompositeDisposable);
            okCommand.Observe.Subscribe(_ => OnOk()).CompositeDisposalWith(okCommandCompositeDisposable);

            this.OkCommand = okCommand;
        }

        private string? _path;
        private IPasswordProtectedFileAccessViewModel? _passwordConfiguration;

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

        public abstract IRxRelayCommand BrowseCommand { get; set; }

        public IPasswordProtectedFileAccessViewModel? PasswordConfiguration
        {
            get => _passwordConfiguration;
            private set
            {
                if (_passwordConfiguration == value) return;
                _passwordConfiguration = value;
                OnPropertyChanged();
            }
        }
        public IFileAccessConfiguration GenerateConfiguration()
        {
            if (this.Path is null) throw new FileNotFoundException();

            return PasswordConfiguration is { IsEncryptionActive: true, Password: {} password}
                ? _createFileAccessConfiguration.CreateWithEncryption(Path, password)
                : _createFileAccessConfiguration.Create(Path);
        }
    }

    public interface INewFileAccessViewModel : IFileAccessViewModel
    {
    }

    internal sealed class NewFileAccessViewModel : FileAccessViewModel, INewFileAccessViewModel
    {
        public NewFileAccessViewModel(
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            Func<IBffSaveFileDialog> bffSaveFileDialogFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration,
            ILocalizer localizer) 
            : base(
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

        public override IRxRelayCommand BrowseCommand { get; set; }
    }

    public interface IOpenFileAccessViewModel : IFileAccessViewModel
    {
    }

    internal sealed class OpenFileAccessViewModel : FileAccessViewModel, IOpenFileAccessViewModel
    {
        public OpenFileAccessViewModel(
            Func<IPasswordProtectedFileAccessViewModel> passwordProtectedFileAccessViewModelFactory,
            Func<IBffOpenFileDialog> bffOpenFileDialogFactory,
            ICreateFileAccessConfiguration createFileAccessConfiguration,
            ILocalizer localizer)
            : base(passwordProtectedFileAccessViewModelFactory, 
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

        public override IRxRelayCommand BrowseCommand { get; set; }
    }
}
