using BFF.Core.Helper;
using Microsoft.Win32;

namespace BFF.View.Helper
{
    class BffSaveFileDialog : IBffSaveFileDialog
    {
        public BffSaveFileDialog()
        {
            _saveFileDialog = new SaveFileDialog();
        }

        private readonly SaveFileDialog _saveFileDialog;
        public bool? ShowDialog()
        {
            return _saveFileDialog.ShowDialog();
        }

        public string DefaultExt
        {
            get => _saveFileDialog.DefaultExt;
            set => _saveFileDialog.DefaultExt = value;
        }

        public string FileName
        {
            get => _saveFileDialog.FileName;
            set => _saveFileDialog.FileName = value;
        }

        public string Filter
        {
            get => _saveFileDialog.Filter;
            set => _saveFileDialog.Filter = value;
        }

        public string Title
        {
            get => _saveFileDialog.Title;
            set => _saveFileDialog.Title = value;
        }
    }
}
