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
        public PayeeViewModel(IPayee payee, IBffOrm orm) : base(orm, payee)
        {
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value) && (CommonPropertyProvider?.AllPayeeViewModels.All(apvm => apvm.Name != Name) ?? false);
        }

        #endregion
    }
}