using System.ComponentModel;

namespace BFF.MVVM.Models.Native.Utility
{
    public interface ICsvBankStatementImportProfile : INotifyPropertyChanged
    {
        string Name { get; }
        string Header { get; set; }
        char Delimiter { get; set; }
        string DateFormat { get; set; }
        string PayeeFormat { get; set; }
        bool ShouldCreateNewPayeeIfNotExisting { get; set; }
        string MemoFormat { get; set; }
        string SumFormat { get; set; }
    }

    public class CsvBankStatementImportProfile : ObservableObject, ICsvBankStatementImportProfile
    {
        private string _header;
        private char _delimiter;
        private string _dateFormat;
        private string _payeeFormat;
        private bool _shouldCreateNewPayeeIfNotExisting;
        private string _memoFormat;
        private string _sumFormat;

        public CsvBankStatementImportProfile(
            string header, 
            char delimiter, 
            string dateFormat,
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat, 
            string sumFormat, 
            string name)
        {
            _header = header;
            _delimiter = delimiter;
            _dateFormat = dateFormat;
            _payeeFormat = payeeFormat;
            _shouldCreateNewPayeeIfNotExisting = shouldCreateNewPayeeIfNotExisting;
            _memoFormat = memoFormat;
            _sumFormat = sumFormat;
            Name = name;
        }

        public string Name { get; }

        public string Header
        {
            get => _header;
            set
            {
                if (_header == value) return;
                _header = value;
                OnPropertyChanged();
            }
        }

        public char Delimiter
        {
            get => _delimiter;
            set
            {
                if (_delimiter == value) return;
                _delimiter = value;
                OnPropertyChanged();
            }
        }

        public string DateFormat
        {
            get => _dateFormat;
            set
            {
                if (_dateFormat == value) return;
                _dateFormat = value;
                OnPropertyChanged();
            }
        }

        public string PayeeFormat
        {
            get => _payeeFormat;
            set
            {
                if (_payeeFormat == value) return;
                _payeeFormat = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldCreateNewPayeeIfNotExisting
        {
            get => _shouldCreateNewPayeeIfNotExisting;
            set
            {
                if (_shouldCreateNewPayeeIfNotExisting == value) return;
                _shouldCreateNewPayeeIfNotExisting = value;
                OnPropertyChanged();
            }
        }

        public string MemoFormat
        {
            get => _memoFormat;
            set
            {
                if (_memoFormat == value) return;
                _memoFormat = value;
                OnPropertyChanged();
            }
        }

        public string SumFormat
        {
            get => _sumFormat;
            set
            {
                if (_sumFormat == value) return;
                _sumFormat = value;
                OnPropertyChanged();
            }
        }
    }
}
