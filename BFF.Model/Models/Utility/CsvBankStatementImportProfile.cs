using System.ComponentModel;
using MrMeeseeks.Windows;

namespace BFF.Model.Models.Utility
{
    public interface ICsvBankStatementImportProfile : INotifyPropertyChanged
    {
        string Name { get; }
        string Header { get; set; }
        char Delimiter { get; set; }
        string? DateSegment { get; set; }
        string DateLocalization { get; set; }
        string PayeeFormat { get; set; }
        bool ShouldCreateNewPayeeIfNotExisting { get; set; }
        string MemoFormat { get; set; }
        string SumFormat { get; set; }
        string SumLocalization { get; set; }
    }

    internal class CsvBankStatementImportProfile : ObservableObject, ICsvBankStatementImportProfile
    {
        private string _header;
        private char _delimiter;
        private string _dateLocalization;
        private string _payeeFormat;
        private bool _shouldCreateNewPayeeIfNotExisting;
        private string _memoFormat;
        private string _sumFormat;
        private string _sumLocalization;
        private string? _dateSegment;

        public CsvBankStatementImportProfile(
            string header, 
            char delimiter,
            string? dateSegment,
            string dateLocalization,
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat, 
            string sumFormat, 
            string sumLocalization,
            string name)
        {
            _header = header;
            _delimiter = delimiter;
            _dateSegment = dateSegment;
            _dateLocalization = dateLocalization;
            _payeeFormat = payeeFormat;
            _shouldCreateNewPayeeIfNotExisting = shouldCreateNewPayeeIfNotExisting;
            _memoFormat = memoFormat;
            _sumFormat = sumFormat;
            _sumLocalization = sumLocalization;
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

        public string? DateSegment
        {
            get => _dateSegment;
            set {
                if (_dateSegment == value) return;
                _dateSegment = value;
                OnPropertyChanged();
            }
        }

        public string DateLocalization
        {
            get => _dateLocalization;
            set
            {
                if (_dateLocalization == value) return;
                _dateLocalization = value;
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

        public string SumLocalization
        {
            get => _sumLocalization;
            set
            {
                if (_sumFormat == value) return;
                _sumLocalization = value;
                OnPropertyChanged();
            }
        }
    }
}
