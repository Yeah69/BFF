namespace BFF.Core.Helper
{
    public interface IBffSaveFileDialog
    {
        bool? ShowDialog();
        string DefaultExt { get; set; }
        string FileName { get; set; }
        string Filter { get; set; }
        string Title { get; set; }
    }
}