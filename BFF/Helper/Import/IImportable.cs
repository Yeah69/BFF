namespace BFF.Helper.Import
{
    interface IImportable
    {
        string SavePath { get; set; }

        string Import();
    }
}
