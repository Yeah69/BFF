using System.Linq;
using System.Windows.Input;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class TitNoTransfer : TitBase
    {
        [Write(false)]
        public string PayeeText { get; set; }

        [Write(false)]
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            Database?.Insert(new Payee {Name = PayeeText});
        }, obj =>
        {
            return PayeeText.Trim() != "" && (Database?.AllPayees)?.Count(payee => payee.Name == PayeeText) == 0;
        });
    }
}
