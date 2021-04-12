namespace BFF.Model.Contexts
{
    internal class EmptyContext : IContext
    {
        public void Dispose()
        {
        }

        public string Title { get; } = "";
    }
}