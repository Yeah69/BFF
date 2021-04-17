using System.Runtime.Serialization;

namespace BFF.ViewModel.ViewModels.Import
{
    public enum ImportOption
    {
        [EnumMember(Value = "Sqlite Project")]
        SqliteProject,
        [EnumMember(Value = "YNAB 4 CSV")]
        Ynab4Csv
    }
}
