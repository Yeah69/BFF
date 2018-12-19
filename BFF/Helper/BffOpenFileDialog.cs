using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFF.Core.Helper;
using Microsoft.Win32;

namespace BFF.Helper
{
    class BffOpenFileDialog : IBffOpenFileDialog
    {
        private readonly OpenFileDialog _openFileDialog;

        public bool? ShowDialog()
        {
            return _openFileDialog.ShowDialog();
        }

        public string DefaultExt
        {
            get => _openFileDialog.DefaultExt;
            set => _openFileDialog.DefaultExt = value;
        }

        public string FileName
        {
            get => _openFileDialog.FileName;
            set => _openFileDialog.FileName = value;
        }

        public string Filter
        {
            get => _openFileDialog.Filter;
            set => _openFileDialog.Filter = value;
        }

        public string Title
        {
            get => _openFileDialog.Title;
            set => _openFileDialog.Title = value;
        }

        public bool Multiselect
        {
            get => _openFileDialog.Multiselect;
            set => _openFileDialog.Multiselect = value;
        }

        public BffOpenFileDialog()
        {
            _openFileDialog = new OpenFileDialog();
        }
    }
}
