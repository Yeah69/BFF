namespace BFF.Persistence.Import
{
    internal interface IImporter
    {
        DtoImportContainer Import();
    }
}