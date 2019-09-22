using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IPasswordProtectedFileAccessViewModel
    {
        bool IsEncryptionActive { get; set; }
        string Password { get; set; }
    }

    internal class PasswordProtectedFileAccessViewModel : ViewModelBase, IPasswordProtectedFileAccessViewModel
    {
        private bool _isEncryptionActive = false;
        private string _password = null;

        public bool IsEncryptionActive
        {
            get => _isEncryptionActive;
            set
            {
                if (value == _isEncryptionActive) return;
                _isEncryptionActive = value;
                if (_isEncryptionActive && Password is null)
                    Password = string.Empty;
                else if (_isEncryptionActive.Not())
                    Password = null;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password) return;
                _password = value;
                if (Password != null) IsEncryptionActive = true;
                OnPropertyChanged();
            }
        }
    }
}