using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel
    {
    }

    public class PayeeViewModel : CommonPropertyViewModel, IPayeeViewModel
    {
        private readonly IPayee _payee;

        public override string Name
        {
            get { return _payee.Name; }
            set
            {
                if (_payee.Name == value) return;
                Update();
                _payee.Name = value;
                OnPropertyChanged();
            }
        }

        public PayeeViewModel(IPayee payee, IBffOrm orm) : base(orm)
        {
            _payee = payee;
        }

        #region Overrides of DataModelViewModel

        public override long Id => _payee.Id;

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name) && (Orm?.CommonPropertyProvider?.AllPayeeViewModels.All(apvm => apvm.Name != Name) ?? false);
        }

        protected override void InsertToDb()
        {
            Orm?.CommonPropertyProvider?.Add(_payee);
        }

        protected override void UpdateToDb()
        {
            _payee.Update(Orm);
        }

        protected override void DeleteFromDb()
        {
            Orm?.CommonPropertyProvider?.Remove(_payee);
        }

        #endregion
    }
}