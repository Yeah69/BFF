namespace BFF.Core.Helper
{
    public interface ILocalizer
    {
        string Localize(string key);

        T Localize<T>(string key);
    }
}
