namespace BFF.Model.Contexts
{
    public interface ILoadContext : IContext
    {
        ILoadContextViewModelProxy ViewModelContext(IContext context);
    }
}