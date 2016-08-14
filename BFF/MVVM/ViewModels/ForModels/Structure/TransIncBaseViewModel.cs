using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TransIncBaseViewModel : TitBaseViewModel
    {
        public abstract Account Account { get; set; }
        public abstract Payee Payee { get; set; }

        protected TransIncBaseViewModel(IBffOrm orm) : base(orm) {}
        
        public string PayeeText { get; set; }
        
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            Payee newPayee = new Payee { Name = PayeeText.Trim() };
            Orm?.Insert(newPayee);
            OnPropertyChanged();
            Payee = newPayee;
        }, obj =>
        {
            string trimmedPayeeText = PayeeText?.Trim();
            return !string.IsNullOrEmpty(trimmedPayeeText) && AllPayees.Count(payee => payee.Name == trimmedPayeeText) == 0;
        });

        public ObservableCollection<Payee> AllPayees => Orm?.CommonPropertyProvider.Payees;

        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });
    }
}
