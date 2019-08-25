using System.ComponentModel;
using System.Runtime.Serialization;

namespace BFF.ViewModel.ViewModels.Import
{
    public enum ImportOption
    {
        [EnumMember(Value = "YNAB 4 CSV")]
        Ynab4Csv
    }
}
